using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class LevelTimerPresenter : MonoBehaviour
{
  [SerializeField]
  Text timerText;
  [SerializeField]
  Image timerImage;
  [SerializeField]
  Image dangerTimerImage;
  [SerializeField]
  Text dangerText;

  ReactiveProperty<float> wholeTimer;
  LevelManager gm;
  Sequence dangerSeq;
  void Awake()
  {
    gm = LevelManager.Instance;
    wholeTimer =
    gm
      .LevelTimer
      .Select(t => Mathf.Ceil(t))
      .ToReactiveProperty();

    wholeTimer
      .Select(t =>
      //      string.Format("{0:D2}'{1:D2}''{2:D2}",
      string.Format("{0:D2}:{1:D2}",
      Mathf.FloorToInt(t / 60),
      Mathf.FloorToInt(t % 60),
      Mathf.FloorToInt((t % 1) * 100)
      )
      )
      .SubscribeToText(timerText)
      .AddTo(this);


    wholeTimer
      .Select(t => t <= 30)
      .Subscribe(b => timerText.color = b ? Color.red : Color.white)
      .AddTo(this);

    wholeTimer
      .Pairwise((a, b) => a > 30 && b <= 30)
      .Where(b => b)
      .Subscribe(_ => AudioManager.Instance.Play(AudioName.EnergyLow))
      .AddTo(this);
    wholeTimer
      .Pairwise((a, b) => a > 0 && b <= 0)
      .Where(b => b)
      .Subscribe(_ => AudioManager.Instance.Play(AudioName.EnergyCritial))
      .AddTo(this);

  }
  void Start()
  {
    Tweener timerTween = null;

    var timeAmount =
    wholeTimer
      .CombineLatest(gm.LevelTimerMax, (l, r) => l / r)
      .ToReactiveProperty();

    timerImage.fillAmount = timeAmount.Value;

    timeAmount
      .Subscribe(t =>
      {
        if (timerTween != null)
        {
          timerTween.Kill();
        }
        //to show full-filled image, fast tween if the value is full
        timerTween = timerImage.DOFillAmount(t, (t == 1) ? .5f : 1f).SetEase(Ease.Linear);
//        timerImage.color = Color.HSVToRGB(t * .5f, 1, 1);
      })
      .AddTo(this);

    dangerSeq = DOTween.Sequence();

    dangerSeq
      .Append(dangerText.DOFade(1, .6f).SetEase(Ease.InCubic))
      .Append(dangerText.DOFade(0, .2f).SetEase(Ease.OutCubic))
      .SetLoops(-1);
    dangerSeq.Pause();
    gm
      .DangerTimer
      .CombineLatest(gm.DangerTimerMax, (l, r) => l / r)
      .Subscribe(v =>
      {
        dangerTimerImage.fillAmount = 1 - v;
      })
      .AddTo(this);
    gm
      .DangerTimer
      .Select(d => d > 0)
      .DistinctUntilChanged()
      .Subscribe(isDanger =>
      {
        dangerText.enabled = dangerTimerImage.enabled = isDanger;

        if (isDanger)
        {
          dangerSeq.Restart();
        }
        else
        {
          dangerSeq.Pause();
        }
      })
      .AddTo(this);
  }
}
