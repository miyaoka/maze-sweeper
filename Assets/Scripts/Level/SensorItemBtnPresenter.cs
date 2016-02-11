using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class SensorItemBtnPresenter : MonoBehaviour
{
  [SerializeField]
  Text countText;

  void Awake()
  {
    var btn = GetComponent<Button>();
    var img = GetComponent<Image>();
    btn
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        var b = LevelManager.Instance.IsSelectedSensor.Value;
        LevelManager.Instance.ItemBtnSelectClear();
        LevelManager.Instance.IsSelectedSensor.Value = !b;
      })
      .AddTo(this);

    GameManager.Instance.SensorCount
      .Subscribe(c =>
      {
        countText.text = c == 0 ? "" : string.Format("x{0}", c);
        btn.interactable = c > 0;
      })
      .AddTo(this);

    LevelManager.Instance.IsSelectedSensor
      .Subscribe(b => img.color = b ? Color.red : Color.white)
      .AddTo(this);

  }
}
