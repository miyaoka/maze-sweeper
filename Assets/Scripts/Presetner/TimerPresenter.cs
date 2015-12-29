using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class TimerPresenter : MonoBehaviour
{
  [SerializeField]
  Text timerText;

  void Awake()
  {
    GameManager
      .Instance
      .LevelTimer
      .Select(t =>
      string.Format("{0:D2}'{1:D2}''{2:D2}",
      Mathf.FloorToInt(t / 60),
      Mathf.FloorToInt(t % 60),
      Mathf.FloorToInt((t % 1) * 100)
      )
      )
      .Subscribe(t => timerText.text = t.ToString())
      .AddTo(this);
  }
}
