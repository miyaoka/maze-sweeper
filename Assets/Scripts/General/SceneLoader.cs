using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public enum GameScene { Title, Lobby, SectorList, Level, Camp, Win, Lose, Result };

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

  public void LoadScene(GameScene sceneName)
  {
    StartCoroutine(loadSceneC(sceneName));
  }

  IEnumerator loadSceneC(GameScene sceneName)
  {
    var ao = SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Single);
    while (!ao.isDone)
    {
      //Loading thread will finish when progress reaches 90%
      progress(ao.progress / .9f);
      yield return null;
    }
    progress(1);
    bgImage.DOFade(0, tweenDuration).OnComplete(() => Destroy(gameObject));
  }
  void progress(float p)
  {
//    loadingText.text = string.Format("loading {0:P0}", p);
    if (tw != null)
    {
      tw.Kill();
    }
    tw = loadingBar.DOFillAmount(p, tweenDuration);
  }
}
