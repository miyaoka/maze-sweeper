using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class MedkitItemBtnPresenter : MonoBehaviour
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
        var b = LevelManager.Instance.IsSelectedMedkit.Value;
        LevelManager.Instance.ItemBtnSelectClear();
        LevelManager.Instance.IsSelectedMedkit.Value = !b;
      })
      .AddTo(this);

    GameManager.Instance.MedkitCount
      .Subscribe(c =>
      {
        countText.text = c == 0 ? "" : string.Format("x{0}", c);
        btn.interactable = c > 0;
      })
      .AddTo(this);

    LevelManager.Instance.IsSelectedMedkit
      .Subscribe(b => img.color = b ? Color.red : Color.white)
      .AddTo(this);

  }
}
