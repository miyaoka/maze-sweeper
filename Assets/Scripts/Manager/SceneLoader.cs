using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public enum SceneName { Title, Lobby, SectorList, Level, Camp, Win, Lose, Result };

[Prefab("SceneLoader")]
public class SceneLoader : SingletonMonoBehaviour<SceneLoader> {
  [SerializeField]
  Image bgImage;
  [SerializeField]
  Image loadingBar;
  [SerializeField]
  Text loadingText;

  float tweenDuration = .5f;
  Tween tw = null;

  void Awake () {
    loadingBar.fillAmount = 0;
    loadingText.text = "";

    DontDestroyOnLoad(gameObject);
	}

  public void LoadScene(SceneName sceneName)
  {
    StartCoroutine(loadSceneC(sceneName));
  }

  IEnumerator loadSceneC(SceneName sceneName)
  {
    var ao = SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Single);
    while (!ao.isDone)
    {
      progress(ao.progress);
      yield return null;
    }
    progress(1);
    bgImage.DOFade(0, tweenDuration).OnComplete(() => Destroy(gameObject));
  }
  void progress(float p)
  {
    if (tw != null)
    {
      tw.Kill();
    }
    tw = loadingBar.DOFillAmount(p, tweenDuration);
  }
}
