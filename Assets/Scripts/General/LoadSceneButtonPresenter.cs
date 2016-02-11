using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(Button))]
public class LoadSceneButtonPresenter : MonoBehaviour {

  [SerializeField]
  GameScene sceneName;

  protected virtual void Awake () {
    var btn = GetComponent<Button>();
    btn.OnClickAsObservable()
      .Subscribe(_ =>
      {
        SceneLoader.Instance.LoadScene(sceneName);
      })
      .AddTo(this);
  }
}
