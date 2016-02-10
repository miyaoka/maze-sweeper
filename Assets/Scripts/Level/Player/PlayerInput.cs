using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class PlayerInput : MonoBehaviour
{
  Plane floorPlane = new Plane(Vector3.up, Vector3.zero);

  double doubleTapInterval = 400;//ms

  public IConnectableObservable<Vector2> MoveInput;
  public IConnectableObservable<Unit> ToggleInput;
  public IConnectableObservable<Vector3> ScrollInput;
  public BoolReactiveProperty IsMovable = new BoolReactiveProperty();
  public BoolReactiveProperty IsScrollable = new BoolReactiveProperty();
  public BoolReactiveProperty IsTogglable = new BoolReactiveProperty();
  public KeyCode ToggleKey = KeyCode.M;

  IDisposable sc;

  void Awake()
  {
    setupMoveInput();
    setupToggle();
    setupScroll();
  }
  void Start()
  {

  }
  void setupMoveInput()
  {
    var update = this
      .UpdateAsObservable()
      .Publish();

    var axisV = update
      .Select(_ => Input.GetAxisRaw("Vertical"))
      .Select(v => toDigital(v))
      .DistinctUntilChanged()
      .Where(v => v != 0)
      .Select(v => new Vector2(0, v))
      .Publish();

    var axisH = update
      .Select(_ => Input.GetAxisRaw("Horizontal"))
      .Select(v => toDigital(v))
      .DistinctUntilChanged()
      .Where(v => v != 0)
      .Select(v => new Vector2(v, 0))
      .Publish();

    var axisInput =
    axisV
      .Merge(axisH)
      .Publish();

    var swipeInput =
    Observable
       .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerSwipe += h,
      h => Lean.LeanTouch.OnFingerSwipe -= h)
      .Select(f => f.SwipeDelta)
      .Select(d => {
        return (Mathf.Abs(d.x) > Mathf.Abs(d.y))
        ? new Vector2(toDigital(d.x), 0)
        : new Vector2(0, toDigital(d.y));
      })
      .Publish();

    MoveInput =
    axisInput
      .Merge(swipeInput)
      .Publish();

    update.Connect();
    axisH.Connect();
    axisV.Connect();
    axisInput.Connect();
    swipeInput.Connect();//.AddTo(this);

  //  sc = swipe.Connect();


    IDisposable connect = null;
    IsMovable
      .Subscribe(b =>
      {
        if (b)
        {
          connect = MoveInput.Connect();
        }
        else if(connect != null)
        { 
          connect.Dispose();
        }
      })
      .AddTo(this);
  }

  void setupToggle()
  {
    //get pair of taps, and check older one interval and current one interval. 
    var doubleTapInput = Observable
      .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerTap += h,
      h => Lean.LeanTouch.OnFingerTap -= h)
      .TimeInterval()
      .Buffer(2, 1)
      .Where(t => t[0].Interval.TotalMilliseconds > doubleTapInterval && t[1].Interval.TotalMilliseconds <= doubleTapInterval)
      .Select(_ => Unit.Default)
      .Publish();

    var tapKeyInput = 
    this
      .UpdateAsObservable()
      .Where(_ => Input.GetKeyUp(ToggleKey))
      .Publish();

    doubleTapInput.Connect();//.AddTo(this);
    tapKeyInput.Connect();

    ToggleInput = 
    doubleTapInput
      .Merge(tapKeyInput)
      .Publish();

    IDisposable connect = null;
    IsTogglable
      .Subscribe(b =>
      {
        if (b)
        {
          connect = ToggleInput.Connect();
        }
        else if (connect != null)
        {
          connect.Dispose();
        }
      })
      .AddTo(this);
  }


  void setupScroll()
  {
    IDisposable onDragConnect = null;
    Lean.LeanFinger finger = null;

    var onDrag = this
      .LateUpdateAsObservable()
      .Publish();

    var fingerDown = Observable
      .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerDown += h,
      h => Lean.LeanTouch.OnFingerDown -= h)
      .Subscribe(f =>
      {
        finger = f;
        onDragConnect = onDrag.Connect();
      })
      .AddTo(this);

    var fingerUp = Observable
      .FromEvent<Lean.LeanFinger>(
      h => Lean.LeanTouch.OnFingerUp += h,
      h => Lean.LeanTouch.OnFingerUp -= h)
      .Publish();

    fingerUp.Connect();

    fingerUp
      .Subscribe(_ =>
      {
        if (onDragConnect != null)
        {
          onDragConnect.Dispose();
        }
      });

    ScrollInput =
      onDrag
      .Select(_ => getFloorHitPoint(finger.GetRay()))
      .DistinctUntilChanged()
      .Pairwise((a, b) => b - a)
      .TakeUntil(fingerUp)
      .Repeat()
      .Publish();

  IDisposable connect = null;
    IsScrollable
      .Subscribe(b =>
      {
        if (b)
        {
          connect = ScrollInput.Connect();
        }
        else if (connect != null)
        {
          connect.Dispose();
        }
      })
      .AddTo(this);

  }


  Vector3 getFloorHitPoint(Ray ray)
  {
    float enter;
    Vector3 v = Vector3.zero;
    if (floorPlane.Raycast(ray, out enter))
    {
      v = ray.GetPoint(enter);
    }
    return v;
  }
  int toDigital(float v)
  {
    return v == 0 ? 0 : v > 0 ? 1 : -1;
  }
  void OnDestroy()
  {
    if(sc != null)
    {
      sc.Dispose();
    }

  }
}
