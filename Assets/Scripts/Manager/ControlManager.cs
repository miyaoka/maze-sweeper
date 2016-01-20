﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class ControlManager : MonoBehaviour
{
  PlayerManager pm;
  IConnectableObservable<Unit> update;
  System.IDisposable connect;
  [SerializeField]
  Transform gridPivot;

  Vector3 initialHit;
  Plane floorPlane = new Plane(Vector3.up, Vector3.zero);

  CompositeDisposable mapResources = new CompositeDisposable();
  void Awake()
  {
    pm = PlayerManager.Instance;
    update = this
      .UpdateAsObservable()
      .Publish();
  }
  void Start()
  {
    var gm = GameManager.Instance;

    //move control
    update
      .Select(up => Input.GetAxisRaw("Vertical"))
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
    update
      .Select(up => Input.GetAxisRaw("Horizontal"))
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

    //toggle map
    this
      .UpdateAsObservable()
      .Where(_ => Input.GetKeyUp(KeyCode.M))
      .Subscribe(_ => gm.ViewState.Value = gm.ViewState.Value == ViewStateName.Map ? ViewStateName.Move : ViewStateName.Map)
      .AddTo(this);

    gm.ViewState
      .Subscribe(v =>
      {
        if (gm.ViewState.Value == ViewStateName.Map)
        {
          Lean.LeanTouch.OnFingerDown += onFingerDown;
          Lean.LeanTouch.OnFingerUp += onFingerUp;
        }
        else
        {
          mapResources.Clear();
          Lean.LeanTouch.OnFingerDown -= onFingerDown;
          Lean.LeanTouch.OnFingerUp -= onFingerUp;
          gridPivot.DOLocalMove(Vector3.zero, .2f).SetEase(Ease.OutQuad);
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
  protected virtual void OnEnable()
  {
    connect = update.Connect();
    Lean.LeanTouch.OnFingerSwipe += OnFingerSwipe;
  }

  protected virtual void OnDisable()
  {
    connect.Dispose();
    Lean.LeanTouch.OnFingerSwipe -= OnFingerSwipe;
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
