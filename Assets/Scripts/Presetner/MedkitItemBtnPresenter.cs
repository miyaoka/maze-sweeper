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
        var b = RoundManager.Instance.IsSelectedMedkit.Value;
        RoundManager.Instance.ItemBtnSelectClear();
        RoundManager.Instance.IsSelectedMedkit.Value = !b;
      })
      .AddTo(this);

    GameManager.Instance.MedkitCount
      .Subscribe(c =>
      {
        countText.text = c == 0 ? "" : string.Format("x{0}", c);
        btn.interactable = c > 0;
      })
      .AddTo(this);

    RoundManager.Instance.IsSelectedMedkit
      .Subscribe(b => img.color = b ? Color.red : Color.white)
      .AddTo(this);

  }
}
