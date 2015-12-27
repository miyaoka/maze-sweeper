using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AlertPresenter : MonoBehaviour
{
  [SerializeField]
  LayoutElement alertCountLE;
  [SerializeField]
  Image alertImage;
  [SerializeField]
  GameObject alert;
  [SerializeField]
  GameObject danger;
  void Start()
  {
    var gm = GameManager.Instance;
    var seq = DOTween.Sequence();
    seq
      .Append(alertImage.DOFade(1, .5f).SetEase(Ease.InCubic))
      .Append(alertImage.DOFade(0, .5f).SetEase(Ease.InCubic))
      .SetLoops(-1);

    gm.alertCount
      .Subscribe(c =>
      {
        alertCountLE.preferredWidth = 0;

        DOTween.To(() => alertCountLE.preferredWidth, x => alertCountLE.preferredWidth = x, 20 * c, .4f)
          .SetEase(Ease.OutExpo);
        if (c == 0 || gm.enemyCount.Value != 0)
        {
          seq.Pause();
          //          seq.Complete();
          alert.SetActive(false);
          return;
        }
        seq.Restart();
        alert.SetActive(true);
        for (var i = 0; i < c; i++)
        {
          AudioManager.enemyDetect.Play();
        }
      })
      .AddTo(this);
  }

}
