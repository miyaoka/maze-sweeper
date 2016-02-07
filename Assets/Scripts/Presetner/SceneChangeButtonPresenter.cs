using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;

public class SceneChangeButtonPresenter : MonoBehaviour {
  [SerializeField]
  SceneName sceneName;

	void Start () {
    DontDestroyOnLoad(transform.root.gameObject);

    GetComponentInChildren<Text>().text = sceneName.ToString();
    GetComponent<Button>()
      .OnClickAsObservable()
      .Subscribe(_ => SceneLoader.Instance.LoadScene(sceneName));
	
	}
}
