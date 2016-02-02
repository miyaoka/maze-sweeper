using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class BombBtnPresenter : MonoBehaviour
{
  [SerializeField]
  Button btn;
  [SerializeField]
  Text btnText;

  void Awake()
  {
    btn
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        FloorManager.Instance.OnBomb.Value = !FloorManager.Instance.OnBomb.Value;
      })
      .AddTo(this);

    FloorManager.Instance.OnBomb
      .Subscribe(b => btnText.color = b ? Color.red : Color.white)
      .AddTo(this);

  }
}
