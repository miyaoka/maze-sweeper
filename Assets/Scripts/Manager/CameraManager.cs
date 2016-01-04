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
  float[] heights = { 35, 125, 0 };
  float baseRatio = 16f / 9f;
  ReactiveProperty<float> aspect = new ReactiveProperty<float>(1);

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    gm = GraphManager.Instance;
  }
  void Start()
  {
    var gm = GameManager.Instance;
    aspect = mainCam
      .ObserveEveryValueChanged(c => c.aspect)
      .ToReactiveProperty();

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
      .Subscribe(_ => mapView.Value = !mapView.Value)
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

