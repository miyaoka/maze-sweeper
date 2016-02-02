using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
public class MapBtnPresenter : MonoBehaviour
{
  void Start()
  {
    var gm = FloorManager.Instance;
    var btn = GetComponent<Button>();
    var text = GetComponentInChildren<Text>();

    btn
      .OnClickAsObservable()
      .Subscribe(b =>
      {
        gm.ViewState.Value = gm.ViewState.Value == ViewStateName.Map ? ViewStateName.Move : ViewStateName.Map;
      })
      .AddTo(this);

    gm.ViewState
      .Subscribe(v =>
      {
        text.text = gm.ViewState.Value == ViewStateName.Map ? "move" : "map";
      })
      .AddTo(this);

    gm.ViewState
      .Select(v => v == ViewStateName.Battle)
      .Subscribe(b =>
      {
      })
      .AddTo(this);
  }
}
