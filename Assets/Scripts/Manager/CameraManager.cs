using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;

public class CameraManager : SingletonMonoBehaviour<CameraManager>{

  [SerializeField] Transform cameraPivot;
  [SerializeField] Camera cam;
  GridManager gm;
  float[] heights = {25, 100, 0};
  float baseRatio = 16f / 9f;
  ReactiveProperty<float> aspect = new ReactiveProperty<float> (1);

  void Awake ()
  {
    if (this != Instance) {
      Destroy (this);
      return;
    }
    DontDestroyOnLoad (this.gameObject);
    gm = GridManager.Instance;
  }
  void Start(){
    var gm = GameManager.Instance;
    aspect = cam
      .ObserveEveryValueChanged (c => c.aspect)
      .ToReactiveProperty ();

    gm.viewState
      .CombineLatest(aspect, (v,a) => heights[(int)v] * Mathf.Pow(baseRatio / a, .5f))
      .Subscribe (f => {
        moveDist(f);
      })
      .AddTo (this);
  }
  //keep rot and go backward
  void moveDist(float dist){
    var pos = cam.transform.forward * -dist;
    cam.transform
      .DOLocalMove (pos, .2f)
      .SetEase (Ease.OutQuad);

  }
  public void movePos(IntVector2 dest){
    cameraPivot.transform
      .DOLocalMove (new Vector3 (dest.x * gm.gridUnit, cameraPivot.transform.localPosition.y, dest.y * gm.gridUnit), .2f)
      .SetEase (Ease.OutQuad);
  }
  public void moveMap(){
  }
}

