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
        RoundManager.Instance.OnBomb.Value = !RoundManager.Instance.OnBomb.Value;
      })
      .AddTo(this);

    RoundManager.Instance.OnBomb
      .Subscribe(b => btnText.color = b ? Color.red : Color.white)
      .AddTo(this);

  }
}
