using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class SensorTargetBtnPresenter : MonoBehaviour
{
  public Node Node;

  void Awake()
  {
    var btn = GetComponent<Button>();
    var img = GetComponent<Image>();
    var graph = GraphManager.Instance;
    btn
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        graph.ShowNode(Node.Coords);
        graph.ScanEnemies(Node);
        RoundManager.Instance.IsSelectedSensor.Value = false;
        GameStateManager.Instance.SensorCount.Value -= 1;
      })
      .AddTo(this);

    RoundManager.Instance.IsSelectedSensor
      .Where(s => !s)
      .Subscribe(b => Destroy(gameObject))
      .AddTo(this);

  }
}
