using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class TimerPresenter : MonoBehaviour
{
  [SerializeField]
  Text timerText;
  [SerializeField]
  Image timerImage;

  void Awake()
  {
    var gm = GameManager.Instance;

    gm
      .LevelTimer
      .Select(t =>
      string.Format("{0:D2}'{1:D2}''{2:D2}",
      Mathf.FloorToInt(t / 60),
      Mathf.FloorToInt(t % 60),
      Mathf.FloorToInt((t % 1) * 100)
      )
      )
      .SubscribeToText(timerText)
      .AddTo(this);

    gm
      .LevelTimer
      .CombineLatest(gm.LevelTimerMax, (l, r) => l / r)
      .Subscribe(t => timerImage.fillAmount = t)
      .AddTo(this);
  }
}
