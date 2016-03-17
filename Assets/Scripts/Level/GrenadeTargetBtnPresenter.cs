using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class GrenadeTargetBtnPresenter : MonoBehaviour
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
        graph.BreachNode(Coords);
        LevelManager.Instance.IsSelectedGrenade.Value = false;
        GameManager.Instance.GrenadeCount.Value -= 1;
      })
      .AddTo(this);

    LevelManager.Instance.IsSelectedGrenade
      .Where(s => !s)
      .Subscribe(b => Destroy(gameObject))
      .AddTo(this);

  }
}
