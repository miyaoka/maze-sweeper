using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

[RequireComponent (typeof (Button))]
public class ZoomPresenter : MonoBehaviour {
	[SerializeField] Button btn;
	[SerializeField] Text btnText;
	ReactiveProperty<bool> onZoomIn = new ReactiveProperty<bool>(true);

	void Start () {

		btn
			.OnClickAsObservable()
			.Subscribe (b => {
				onZoomIn.Value = !onZoomIn.Value;
			})
			.AddTo(this); 

		onZoomIn
			.Subscribe (b => {
				var scale = b ? 1f : .2f;
				GridManager.Instance.scale(scale);
				btnText.text = b ? "map" : "return";
			}).AddTo (this);
	}
}