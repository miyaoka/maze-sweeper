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

  // Use this for initialization
  void Start () {
    var pm = PlayerManager.Instance;


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

    wBtn
      .OnClickAsObservable ()
      .Subscribe (_ => pm.moveDir (Dirs.North))
      .AddTo (this);
    aBtn
      .OnClickAsObservable ()
      .Subscribe (_ => pm.moveDir (Dirs.West))
      .AddTo (this);
    sBtn
      .OnClickAsObservable ()
      .Subscribe (_ => pm.moveDir (Dirs.South))
      .AddTo (this);
    dBtn
      .OnClickAsObservable ()
      .Subscribe (_ => pm.moveDir (Dirs.East))
      .AddTo (this);

  }
}
