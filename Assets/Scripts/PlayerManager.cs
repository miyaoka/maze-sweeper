using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>{

    public ReactiveProperty<IntVector2> currentCoords = new ReactiveProperty<IntVector2> ();
	GridManager gm;
    void Awake ()
    {
        if (this != Instance) {
            Destroy (this);
            return;
        }
        DontDestroyOnLoad (this.gameObject);
    }
	void Start () {
		gm = GridManager.Instance;

		var update = this
			.UpdateAsObservable ();

		//move control
		update
			.Where (up => Input.GetKeyDown (KeyCode.W))
			.Subscribe (_ => moveDir (Dirs.North))
			.AddTo (this);
		update
			.Where (down => Input.GetKeyDown (KeyCode.S))
			.Subscribe (_ => moveDir (Dirs.South))
			.AddTo (this);
		update
			.Where (right => Input.GetKeyDown (KeyCode.D))
			.Subscribe (_ => moveDir (Dirs.East))
			.AddTo (this);
		update
			.Where (left => Input.GetKeyDown (KeyCode.A))
			.Subscribe (_ => moveDir (Dirs.West))
			.AddTo (this);
	}
    public void moveDir(Dirs dir)
    {

        var edge = gm.getEdgeModelByDir (currentCoords.Value, dir);
        if (edge == null) {
            return;
        }
        //      if (!edge.type.Value.isPassable.Value) {
        //          breachEdge(currentCoords.Value.x, currentCoords.Value.y, dir);
        //          return;
        //      }
		movePos (currentCoords.Value + GridManager.dirCoords[(int)dir]);
    }	
    void movePos(IntVector2 dest){
        var node = gm.getNodeModel(dest);
        if (node == null) {
            return;
        }
        node.visited.Value = true;

		/*
        //表示位置を変える
        gridPos
            .DOLocalMove (new Vector3 (-dest.x * gridUnit, -dest.y * gridUnit, 0), .2f)
            .SetEase (Ease.OutQuad)
            .OnComplete (() => {
                currentCoords.Value = dest;

                if (node.enemyCount.Value > 0) {
                    Debug.Log ("enemy:" + node.enemyCount.Value);
                    var ec = node.enemyCount.Value;

                    foreach (var n in node.Neighbors) {
                        if (n == node) {
                            continue;
                        }
                        //TODO:未探索nodeだとマイナスになる
                        //0に補正するか0以下は表示しないか
                        n.alertCount.Value = Mathf.Max(0, n.alertCount.Value - ec);
                    }

                    node.enemyCount.Value = 0;
                    //          ec -= 1;

                    //ランダム位置にワープ
                    //TODO: 同じ位置に飛ばないようにする
                    if (0 < ec) {
                        //                      nodeList[Random.Range(0, nodeList.Count)].enemyCount.Value += ec;
                    }

                }
                gm.alertCount.Value = node.alertCount.Value = node.scanEnemies ();
            });
		*/


    }
}
