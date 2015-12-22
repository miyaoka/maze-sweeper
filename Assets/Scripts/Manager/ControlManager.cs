using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class ControlManager : MonoBehaviour {

  PlayerManager pm;
  IConnectableObservable<Unit> update;
  System.IDisposable connect;
  [SerializeField] Transform gridPivot;
  CompositeDisposable mapResources = new CompositeDisposable();

  void Awake() {
    pm = PlayerManager.Instance;
    update = this
      .UpdateAsObservable ()
      .Publish ();
  }
  void Start () {
    var gm = GameManager.Instance;

    //move control
    update
      .Select (up => Input.GetAxisRaw ("Vertical"))
      .Select (v => v > 0 
        ? Dirs.North
        : v < 0 
        ? Dirs.South 
        : Dirs.Null
      )
      .DistinctUntilChanged ()
      .Where (d => d != Dirs.Null)
      .Subscribe (pm.moveDir)
      .AddTo (this);
    update
      .Select (up => Input.GetAxisRaw ("Horizontal"))
      .Select (v => v > 0 
        ? Dirs.East
        : v < 0 
        ? Dirs.West 
        : Dirs.Null
      )
      .DistinctUntilChanged ()
      .Where (d => d != Dirs.Null)
      .Subscribe (pm.moveDir)
      .AddTo (this);

    //toggle map
    this
      .UpdateAsObservable()
      .Where (_ => Input.GetKeyUp (KeyCode.M))
      .Subscribe (_ => gm.viewState.Value = gm.viewState.Value == ViewState.Map ? ViewState.Move : ViewState.Map)
      .AddTo (this);

    gm.viewState
      .Subscribe (v => {
        if(gm.viewState.Value == ViewState.Map){
          this
            .LateUpdateAsObservable()
            .Subscribe(_ => {
//              var d = Lean.LeanTouch.DragDelta;
              //              gridPivot.localPosition += new Vector3(d.x, 0, d.y);
              Lean.LeanTouch.MoveObject(gridPivot, Lean.LeanTouch.DragDelta);
            })
            .AddTo(mapResources);
        }
        else{
          mapResources.Clear();
          gridPivot.DOLocalMove(Vector3.zero, .2f).SetEase (Ease.OutQuad);
        }
      })
      .AddTo (this);

  }
  protected virtual void OnEnable()
  {
    connect = update.Connect ();
    Lean.LeanTouch.OnFingerSwipe += OnFingerSwipe;
  }

  protected virtual void OnDisable()
  {
    connect.Dispose ();
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
  void move(Dirs dir){
    pm.moveDir (dir);
  }
}
