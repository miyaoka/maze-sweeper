using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

[RequireComponent (typeof (Button))]
public class ZoomPresenter : MonoBehaviour {
  [SerializeField] Button btn;
  [SerializeField] Text btnText;
  ReactiveProperty<bool> onZoomIn = new ReactiveProperty<bool>(true);

  ReactiveProperty<int> zoomLevel = new ReactiveProperty<int>(0);
  void Start () {

    btn
      .OnClickAsObservable()
      .Subscribe (b => {
        zoomLevel.Value = (zoomLevel.Value + 1) % 3;
        onZoomIn.Value = !onZoomIn.Value;
      })
      .AddTo(this); 

    var scales = new float[]{1, .2f, 2};
    var texts = new string[]{ "normal", "map", "zoom" };
    zoomLevel
      .Subscribe (i => {
        CameraManager.Instance.Scale(scales[i]);
        btnText.text = texts[i];

      }).AddTo (this);
    /*
    onZoomIn
      .Subscribe (b => {
        var scale = b ? 1f : .2f;
        CameraManager.Instance.Scale(scale);
        btnText.text = b ? "map" : "return";
      }).AddTo (this);
      */
  }
}