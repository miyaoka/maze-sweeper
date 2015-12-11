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
		transform
			.DOLocalMove (new Vector3 (dest.x * gm.gridUnit, dest.y * gm.gridUnit, -1), .2f)
			.SetEase (Ease.OutQuad);


	}
}
