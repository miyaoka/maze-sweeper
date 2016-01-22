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
  float[] heights = { 35, 125, 15 };
  float baseRatio = 16f / 9f;
  ReactiveProperty<float> aspect = new ReactiveProperty<float>(1);
  Tweener shakeTween;
  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    aspect = mainCam
      .ObserveEveryValueChanged(c => c.aspect)
      .ToReactiveProperty();
  }
  void Start()
  {
    var gm = GameManager.Instance;

    gm.ViewState
      .CombineLatest(aspect, (v, a) => heights[(int)v] * Mathf.Pow(baseRatio / a, .5f))
      .Subscribe(f =>
      {
        moveDist(f);
      })
      .AddTo(this);

    CameraClearFlags skyCamFlags = skyCam.clearFlags;
    float mainCamPos = mainCam.transform.localPosition.y;
    var mainCamRot = mainCam.transform.localRotation.eulerAngles;

    var mapView = new ReactiveProperty<bool>();

    mapView
      .Subscribe(b =>
      {
        if(b)
        {
          skyCamFlags = skyCam.clearFlags;
          mainCamPos = mainCam.transform.localPosition.y;
          mainCamRot = mainCam.transform.localRotation.eulerAngles;

          skyCam.clearFlags = CameraClearFlags.Color;
          bgCam.enabled = false;
          mainCam.transform.DOLocalMoveY(300, .1f);
          mainCam.transform.DOLocalRotate(new Vector3(90, 0, 0), .1f);
        }
        else
        {
          skyCam.clearFlags = skyCamFlags;
          bgCam.enabled = true;
          mainCam.transform.DOLocalMoveY(mainCamPos, .1f);
          mainCam.transform.DOLocalRotate(mainCamRot, .1f);
        }
      })
      .AddTo(this);
    this
      .UpdateAsObservable()
      .Where(_ => Input.GetKeyUp(KeyCode.O))
      .Subscribe(_ => { mapView.Value = !mapView.Value; })
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
    var pos = GraphManager.Instance.CoordsToVec3(dest);
    cameraPivot.transform
      .DOLocalMove(pos, .2f)
      .SetEase(Ease.OutQuad);
  }
  public void shake()
  {
    var pos = GraphManager.Instance.CoordsToVec3(PlayerManager.Instance.CurrentCoords.Value);
    shakeTween.Complete();
    shakeTween = cameraPivot.transform
      .DOLocalMoveX(pos.x + 1f, .05f)
      .SetLoops(2, LoopType.Yoyo)
      .OnComplete(() => cameraPivot.DOMove(pos, 0));
  }
}

