using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class ControlManager : MonoBehaviour {
  [SerializeField] Button wBtn;
  [SerializeField] Button aBtn;
  [SerializeField] Button sBtn;
  [SerializeField] Button dBtn;

  PlayerManager pm;
  void Awake() {
    pm = PlayerManager.Instance;
  }
  void Start () {
    var gm = GameManager.Instance;
    var update = this
      .UpdateAsObservable ();

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

    update
      .Where (_ => Input.GetKeyUp (KeyCode.M))
      .Subscribe (_ => gm.viewState.Value = gm.viewState.Value == ViewState.Map ? ViewState.Move : ViewState.Map)
      .AddTo (this);
        

    wBtn
      .OnClickAsObservable ()
      .Subscribe (_ => move(Dirs.North) )
      .AddTo (this);
    aBtn
      .OnClickAsObservable ()
      .Subscribe (_ => move(Dirs.West) )
      .AddTo (this);
    sBtn
      .OnClickAsObservable ()
      .Subscribe (_ => move(Dirs.South))
      .AddTo (this);
    dBtn
      .OnClickAsObservable ()
      .Subscribe (_ => move(Dirs.East))
      .AddTo (this);

    gm.viewState
      .Subscribe (v => {
        if(gm.viewState.Value == ViewState.Map){
          Lean.LeanTouch.OnFingerSwipe -= OnFingerSwipe;
        }
        else{
          Lean.LeanTouch.OnFingerSwipe += OnFingerSwipe;
        }
      })
      .AddTo (this);

  }
  protected virtual void OnEnable()
  {
    // Hook into the OnSwipe event
//    Lean.LeanTouch.OnFingerSwipe += OnFingerSwipe;
  }

  protected virtual void OnDisable()
  {
    // Unhook into the OnSwipe event
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
