using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;

public enum ViewState { Normal, Map, Battle };
public class CameraManager : SingletonMonoBehaviour<CameraManager>
{
  public Transform cameraPivot;
  public Camera mainCam;
  public Transform GridPivot;

  Dictionary<ViewState, float> heightDict = new Dictionary<ViewState, float>()
  {
    { ViewState.Normal, 45f },
    { ViewState.Map, 150f },
    { ViewState.Battle, 15f }
  };
  
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
    var player = GetComponent<PlayerManager>();
    var gm = LevelManager.Instance;

    gm
      .CurrentView
      .CombineLatest(aspect, (v, a) => heightDict[v] * Mathf.Pow(baseRatio / a, .5f))
      .Subscribe(f =>
      {
        changeHeight(f);
      })
      .AddTo(this);

    var input = GetComponent<PlayerInput>();
    input
      .ScrollInput
      .Subscribe(v =>
      {
        GridPivot.position += v;
      })
      .AddTo(this);

    gm
      .CurrentView
      .Where(v => v == ViewState.Normal)
      .Subscribe(_ =>
      {
        GridPivot
          .DOLocalMove(Vector3.zero, .2f)
          .SetEase(Ease.OutQuad);
      })
      .AddTo(this);
  }
  //keep rot and go backward
  void changeHeight(float height)
  {
    var pos = mainCam.transform.forward * -height;
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

