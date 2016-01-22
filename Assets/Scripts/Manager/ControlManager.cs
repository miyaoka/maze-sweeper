using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using System;

public class ControlManager : MonoBehaviour
{
  PlayerManager pm;
  [SerializeField]
  Transform gridPivot;

  Vector3 initialHit;
  Plane floorPlane = new Plane(Vector3.up, Vector3.zero);

  CompositeDisposable mapResources = new CompositeDisposable();

  IConnectableObservable<Unit> movement;
  IConnectableObservable<Lean.LeanFinger> swipe;
  IConnectableObservable<Lean.LeanFinger> fingerDown;
  IConnectableObservable<Lean.LeanFinger> fingerUp;

  GameManager gm;
  void Awake()
  {
    pm = PlayerManager.Instance;
    gm = GameManager.Instance;
    movement = this
      .UpdateAsObservable()
      .Publish();

    swipe = Observable
      .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerSwipe += h,
      h => Lean.LeanTouch.OnFingerSwipe -= h)
      .Publish();

    fingerDown = Observable
      .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerDown += h,
      h => Lean.LeanTouch.OnFingerDown -= h)
      .Publish();

    fingerUp = Observable
      .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerUp += h,
      h => Lean.LeanTouch.OnFingerUp -= h)
      .Publish();
  }
  void Start()
  {
    //move control
    movement
      .Select(b => Input.GetAxisRaw("Vertical"))
      .Select(v => v > 0
        ? Dirs.North
        : v < 0
        ? Dirs.South
        : Dirs.Null
      )
      .DistinctUntilChanged()
      .Where(d => d != Dirs.Null)
      .Subscribe(pm.MoveDir)
      .AddTo(this);
    movement
      .Select(b => Input.GetAxisRaw("Horizontal"))
      .Select(v => v > 0
        ? Dirs.East
        : v < 0
        ? Dirs.West
        : Dirs.Null
      )
      .DistinctUntilChanged()
      .Where(d => d != Dirs.Null)
      .Subscribe(pm.MoveDir)
      .AddTo(this);

    swipe
      .Subscribe(OnFingerSwipe)
      .AddTo(this);

    //scroll control
    fingerDown
      .Subscribe(onFingerDown)
      .AddTo(this);

    fingerUp
      .Subscribe(onFingerUp)
      .AddTo(this);

    //toggle map
    this
      .UpdateAsObservable()
      .Where(_ => Input.GetKeyUp(KeyCode.M))
      .Subscribe(_ => {
        gm.ViewState.Value = gm.ViewState.Value == ViewStateName.Map ? ViewStateName.Move : ViewStateName.Map;
      })
      .AddTo(this);

    //movement
    var mc = movement.Connect();
    var sc = swipe.Connect();

    gm.ViewState
      .Select(s => s == ViewStateName.Move)
      .CombineLatest(gm.OnMenu, (l, r) => l && !r)
      .Subscribe(onMove =>
      {
        mc.Dispose();
        sc.Dispose();
        if (onMove)
        {
          mc = movement.Connect();
          sc = swipe.Connect();
        }
      })
      .AddTo(this);

    //map scroll
    var fdc = fingerDown.Connect();
    var fuc = fingerUp.Connect();

    gm.ViewState
      .Select(s => s == ViewStateName.Map)
      .CombineLatest(gm.OnMenu, (l, r) => l && !r)
      .Subscribe(onMap =>
      {
        mapResources.Clear();
        fdc.Dispose();
        fuc.Dispose();
        gridPivot.DOLocalMove(Vector3.zero, .2f).SetEase(Ease.OutQuad);
        if (onMap)
        {
          fdc = fingerDown.Connect();
          fuc = fingerUp.Connect();
        }
      })
      .AddTo(this);

  }
  void onFingerDown(Lean.LeanFinger finger)
  {
    var ray = finger.GetRay(); //fires ray with ScreenToWorld at finger pos using LeanTouch
    float enter;
    var initPos = gridPivot.position;
    if (floorPlane.Raycast(ray, out enter))
    {
      initialHit = ray.GetPoint(enter);
    }

    mapResources.Clear();
    this
      .LateUpdateAsObservable()
      .Subscribe(_ =>
      {
        ray = finger.GetRay();
        if (floorPlane.Raycast(ray, out enter))
        {
          var currentHit = ray.GetPoint(enter);
          var directionHit = (currentHit - initialHit);
          gridPivot.position = initPos + new Vector3(directionHit.x, 0, directionHit.z); //camera stays at same height. Invert coords to move camera the correct direction
        }
      })
      .AddTo(mapResources);
  }
  void onFingerUp(Lean.LeanFinger finger)
  {
    mapResources.Clear();
  }
  public void OnFingerSwipe(Lean.LeanFinger finger)
  {
    // Store the swipe delta in a temp variable
    var swipe = finger.SwipeDelta;
    if (swipe.x < -Mathf.Abs(swipe.y))
    {
      move(Dirs.West);
    }

    if (swipe.x > Mathf.Abs(swipe.y))
    {
      move(Dirs.East);
    }

    if (swipe.y < -Mathf.Abs(swipe.x))
    {
      move(Dirs.South);
    }

    if (swipe.y > Mathf.Abs(swipe.x))
    {
      move(Dirs.North);
    }
  }
  void move(Dirs dir)
  {
    pm.MoveDir(dir);
  }
}
