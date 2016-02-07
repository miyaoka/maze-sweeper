using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;

public class TitleButtonPresenter : MonoBehaviour {
	void Start () {
    var gi = GameManager.Instance;
    var btn = GetComponent<Button>();
    btn.OnClickAsObservable()
      .Subscribe(_ =>
      {
        SceneLoader.Instance.LoadScene(SceneName.SectorList);
      })
      .AddTo(this);
	}
}
