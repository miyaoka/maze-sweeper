using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class DirBtnPresenter : MonoBehaviour {
	[SerializeField] Button wBtn;
	[SerializeField] Button aBtn;
	[SerializeField] Button sBtn;
	[SerializeField] Button dBtn;

	// Use this for initialization
	void Start () {
		var pm = PlayerManager.Instance;
		wBtn
			.OnClickAsObservable ()
			.Subscribe (_ => pm.moveDir (Dirs.North))
			.AddTo (this);
		aBtn
			.OnClickAsObservable ()
			.Subscribe (_ => pm.moveDir (Dirs.West))
			.AddTo (this);
		sBtn
			.OnClickAsObservable ()
			.Subscribe (_ => pm.moveDir (Dirs.South))
			.AddTo (this);
		dBtn
			.OnClickAsObservable ()
			.Subscribe (_ => pm.moveDir (Dirs.East))
			.AddTo (this);
				
	}
}
