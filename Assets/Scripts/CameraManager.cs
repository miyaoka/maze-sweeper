using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;

public class CameraManager : SingletonMonoBehaviour<CameraManager>{

  [SerializeField] Transform pivot;
  [SerializeField] Transform grid;
  [SerializeField] Camera cam;
  GridManager gm;
  float[] heights = {25, 100, 0};
  float baseRatio = 16f / 9f;
  ReactiveProperty<float> aspect = new ReactiveProperty<float> (1);

  CompositeDisposable mapResources = new CompositeDisposable();
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

    gm.viewState
      .Subscribe (v => {
        if(gm.viewState.Value == ViewState.Map){
          Observable
            .EveryFixedUpdate()
            .Subscribe(_ => Lean.LeanTouch.MoveObject(grid, Lean.LeanTouch.DragDelta))
            .AddTo(mapResources);
        }
        else{
          mapResources.Clear();
          grid.DOLocalMove(Vector3.zero, .2f).SetEase (Ease.OutQuad);

        }
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
    pivot.transform
      .DOLocalMove (new Vector3 (dest.x * gm.gridUnit, dest.y * gm.gridUnit, pivot.transform.localPosition.z), .2f)
      .SetEase (Ease.OutQuad);
  }
  public void moveMap(){
  }
}

