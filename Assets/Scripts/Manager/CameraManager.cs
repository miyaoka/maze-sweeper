using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;

public class CameraManager : SingletonMonoBehaviour<CameraManager>
{
  [SerializeField]
  Transform cameraPivot;
  [SerializeField]
  Camera mainCam;
  [SerializeField]
  Camera skyCam;
  [SerializeField]
  Camera bgCam;
  GraphManager gm;
  float[] heights = { 30, 200, 0 };
  float baseRatio = 16f / 9f;
  ReactiveProperty<float> aspect = new ReactiveProperty<float>(1);

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    DontDestroyOnLoad(this.gameObject);
    gm = GraphManager.Instance;
  }
  void Start()
  {
    var gm = GameManager.Instance;
    aspect = mainCam
      .ObserveEveryValueChanged(c => c.aspect)
      .ToReactiveProperty();

    gm.viewState
      .CombineLatest(aspect, (v, a) => heights[(int)v] * Mathf.Pow(baseRatio / a, .5f))
      .Subscribe(f =>
      {
        moveDist(f);
      })
      .AddTo(this);

    this
      .UpdateAsObservable()
      .Where(_ => Input.GetKeyUp(KeyCode.O))
      .Subscribe(_ =>
      {
        skyCam.clearFlags = CameraClearFlags.Color;
        bgCam.enabled = false;
        mainCam.transform.DOLocalMoveY(500, .2f);
        mainCam.transform.DOLocalRotate(new Vector3(90, 0, 0), .2f);

      })
      .AddTo(this);
  }
  //keep rot and go backward
  void moveDist(float dist)
  {
    var pos = mainCam.transform.forward * -dist;
    mainCam.transform
      .DOLocalMove(pos, .2f)
      .SetEase(Ease.OutQuad);

  }
  public void MovePos(IntVector2 dest)
  {
    var pos = gm.CoordsToVec3(dest);
    cameraPivot.transform
      .DOLocalMove(pos, .2f)
      .SetEase(Ease.OutQuad);
  }
}

