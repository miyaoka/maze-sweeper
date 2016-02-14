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

  void Awake()
  {
    var gm = LevelManager.Instance;
    var seq = DOTween.Sequence();
    var cg = GetComponent<CanvasGroup>();
    seq
      .Append(alertImage.DOFade(1, .5f).SetEase(Ease.InCubic))
      .Append(alertImage.DOFade(0, .5f).SetEase(Ease.InCubic))
      .SetLoops(-1);
    seq.Pause();

    gm.AlertCount
      .Subscribe(c =>
      {
        if (c == 0)
        {
          alertCountLE.preferredWidth = 0;
          cg.alpha = 0;
          seq.Pause();
          return;
        }
        DOTween.To(() => alertCountLE.preferredWidth, x => alertCountLE.preferredWidth = x, 20 * c, .4f)
          .SetEase(Ease.OutExpo);

        seq.Restart();
        cg.alpha = 1;
      })
      .AddTo(this);
  }

}
