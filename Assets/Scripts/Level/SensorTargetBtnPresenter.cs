﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class SensorTargetBtnPresenter : MonoBehaviour
{
  public IntVector2 Coords;

  void Awake()
  {
    var btn = GetComponent<Button>();
    var img = GetComponent<Image>();
    var graph = GraphManager.Instance;
    btn
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        AudioManager.Instance.Play(AudioName.Powerup);
        graph.graph.CreateNode(Coords, true);
        var n = graph.ShowNode(Coords);
        graph.ScanEnemies(n);
        n.IsVisited.Value = true;
        LevelManager.Instance.IsSelectedSensor.Value = false;
        GameManager.Instance.SensorCount.Value -= 1;
      })
      .AddTo(this);

    LevelManager.Instance.IsSelectedSensor
      .Where(s => !s)
      .Subscribe(b => Destroy(gameObject))
      .AddTo(this);

  }
}
