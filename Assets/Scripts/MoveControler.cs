using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class MoveControler : MonoBehaviour {

    void Start(){
        var gridManager = GetComponent<GridManager> ();

        var update = this
            .UpdateAsObservable ();

        //move control
        update
            .Where (up => Input.GetKeyDown (KeyCode.W))
            .Subscribe (_ => gridManager.moveDir (Dirs.North))
            .AddTo (this);
        update
            .Where (down => Input.GetKeyDown (KeyCode.S))
            .Subscribe (_ => gridManager.moveDir (Dirs.South))
            .AddTo (this);
        update
            .Where (right => Input.GetKeyDown (KeyCode.D))
            .Subscribe (_ => gridManager.moveDir (Dirs.East))
            .AddTo (this);
        update
            .Where (left => Input.GetKeyDown (KeyCode.A))
            .Subscribe (_ => gridManager.moveDir (Dirs.West))
            .AddTo (this);

    }
}
