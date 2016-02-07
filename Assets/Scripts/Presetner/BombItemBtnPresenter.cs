using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class BombItemBtnPresenter : MonoBehaviour
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
        var b = RoundManager.Instance.IsSelectedBomb.Value;
        RoundManager.Instance.ItemBtnSelectClear();
        RoundManager.Instance.IsSelectedBomb.Value = !b;
      })
      .AddTo(this);

    GameManager.Instance.BombCount
      .Subscribe(c =>
      {
        countText.text = c == 0 ? "" : string.Format("x{0}", c);
        btn.interactable = c > 0;
      })
      .AddTo(this);

    RoundManager.Instance.IsSelectedBomb
      .Subscribe(b => img.color = b ? Color.red : Color.white)
      .AddTo(this);

  }
}
