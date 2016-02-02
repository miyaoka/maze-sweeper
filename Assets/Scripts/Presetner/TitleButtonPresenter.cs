using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;

public class TitleButtonPresenter : MonoBehaviour {
	void Start () {
    var btn = GetComponent<Button>();
    btn.OnClickAsObservable()
      .Subscribe(_ =>
      {
        GameStateManager.Instance.Next();
      })
      .AddTo(this);
	}
}
