using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>{
	[SerializeField] GameObject player;
    public ReactiveProperty<IntVector2> currentCoords = new ReactiveProperty<IntVector2> ();
	public ReactiveProperty<IntVector2> destCoords = new ReactiveProperty<IntVector2> ();
	Sequence sq;

    void Awake ()
    {
        if (this != Instance) {
            Destroy (this);
            return;
        }
        DontDestroyOnLoad (this.gameObject);
    }
	void Start () {
//		sq = DOTween.Sequence ();
//		movePos (currentCoords.Value);
//		movePos (new IntVector2 (3, 3));

	}
    public void moveDir(Dirs dir)
    {
		if (sq != null){
			if (sq.IsPlaying ()) {
				currentCoords.Value = destCoords.Value;
			}
		}
		var gm = GridManager.Instance;

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
    public void movePos(IntVector2 dest){
		var gm = GridManager.Instance;
		var node = gm.getNodeModel(dest);
        if (node == null) {
            return;
        }
		destCoords.Value = dest;

		if (sq != null){
			if (sq.IsPlaying ()) {
//				return;
			}
		}

		node.visited.Value = true;

		CameraManager.Instance.movePos (dest);

		if (sq != null) {
			sq.Kill ();
		}
		sq = DOTween.Sequence();

		AudioManager.door.Play ();
		AudioManager.walk.PlayDelayed (.4f);
//		AudioManager.enemyDetect.Play ();


		sq.Append (player.transform
            .DOLocalMove (new Vector3 (dest.x * gm.gridUnit, dest.y * gm.gridUnit, 0), .8f)
			.SetEase (Ease.OutQuad)
		);
		sq.OnKill (() => {
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
				AudioManager.maleScream.Play();

				//ランダム位置にワープ
				//TODO: 同じ位置に飛ばないようにする
				if (0 < ec) {
					//                      nodeList[Random.Range(0, nodeList.Count)].enemyCount.Value += ec;
				}

			}
			GameManager.Instance.alertCount.Value = node.alertCount.Value = node.scanEnemies ();
		});


    }
}
