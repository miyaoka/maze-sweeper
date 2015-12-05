using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class AlertPresenter : MonoBehaviour {
	[SerializeField] LayoutElement alertCountLE;
	[SerializeField] GameObject alert;
	[SerializeField] GameObject danger;
	void Start () {
	
		var gm = GameManager.Instance;
		gm.alertCount
			.Subscribe (c => {
				alertCountLE.preferredWidth = 20 * c;
				if(c == 0 || gm.enemyCount.Value != 0){
					alert.SetActive(false);
					return;
				}
				alert.SetActive(true);
				for(var i = 0; i< c; i++){
					AudioManager.enemyDetect.Play();
				}
		})
			.AddTo (this);
	}
	
}
