using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CameraManager : SingletonMonoBehaviour<CameraManager>{
	Camera cam;
	float baseSize;
	GridManager gm;
	void Awake ()
	{
		if (this != Instance) {
			Destroy (this);
			return;
		}
		DontDestroyOnLoad (this.gameObject);
	}
	void Start(){
		cam = GetComponent<Camera> ();
		gm = GridManager.Instance;
		baseSize = gm.gridUnit * 1.5f;
	}
	public void Scale(float scale){
		DOTween.To (() => cam.orthographicSize, x => cam.orthographicSize = x, baseSize / scale, .2f)
			.SetEase (Ease.OutQuad);
	}
	public void movePos(IntVector2 dest){
		var node = gm.getNodeModel(dest);
		if (node == null) {
			return;
		}
		node.visited.Value = true;

		//表示位置を変える
		transform
			.DOLocalMove (new Vector3 (dest.x * gm.gridUnit, dest.y * gm.gridUnit, -1), .2f)
			.SetEase (Ease.OutQuad)
			.OnComplete (() => {
				/*
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
				GameManager.Instance.alertCount.Value = node.alertCount.Value = node.scanEnemies ();
				*/
			});


	}
}
