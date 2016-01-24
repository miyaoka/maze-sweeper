using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class TimerPresenter : MonoBehaviour
{
  [SerializeField]
  Text timerText;
  [SerializeField]
  Image timerImage;

  ReactiveProperty<float> wholeTimer;
  GameManager gm;
  void Awake()
  {
    gm = GameManager.Instance;
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
      .Subscribe(b => timerText.color = b ? Color.red : Color.black)
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
      })
      .AddTo(this);
  }
}
