using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
public class MapBtnPresenter : MonoBehaviour
{
  void Start()
  {
    var gm = GameManager.Instance;
    var btn = GetComponent<Button>();
    var text = GetComponentInChildren<Text>();

    btn
      .OnClickAsObservable()
      .Subscribe(b =>
      {
        gm.viewState.Value = gm.viewState.Value == ViewState.Map ? ViewState.Move : ViewState.Map;
      })
      .AddTo(this);

    gm.viewState
      .Subscribe(v =>
      {
        text.text = gm.viewState.Value == ViewState.Map ? "move" : "map";
      })
      .AddTo(this);

    gm.viewState
      .Select(v => v == ViewState.Battle)
      .Subscribe(b =>
      {
      })
      .AddTo(this);
  }
}
