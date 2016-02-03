using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;
using DG.Tweening;

public class ExitProgressPresenter : MonoBehaviour
{
  [SerializeField]
  Image progressBar;
  void Start()
  {
    progressBar.fillAmount = 0;

    Tween tw = null;
    RoundManager.Instance.ExitProgress
      .Subscribe(r =>
      {
        if(tw != null)
        {
          tw.Kill();
        }
        tw = progressBar.DOFillAmount(r, 1f).SetEase(Ease.OutSine);
      })
      .AddTo(this);
  }
}
