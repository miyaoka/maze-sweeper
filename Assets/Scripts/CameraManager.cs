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
//    baseSize = gm.gridUnit * 1.5f;
    baseSize = cam.fieldOfView;
  }
  public void Scale(float scale){
//    DOTween.To (() => cam.orthographicSize, x => cam.orthographicSize = x, baseSize / scale, .2f)
    DOTween.To (() => cam.fieldOfView, x => cam.fieldOfView = x, Mathf.Min(140f, baseSize / scale), .2f)
      .SetEase (Ease.OutQuad);
  }
  public void movePos(IntVector2 dest){
    transform
      .DOLocalMove (new Vector3 (dest.x * gm.gridUnit, dest.y * gm.gridUnit -10, transform.localPosition.z), .2f)
      .SetEase (Ease.OutQuad);


  }
}
