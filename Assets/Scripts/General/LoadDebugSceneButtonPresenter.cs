using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(Button))]
public class LoadDebugSceneButtonPresenter : MonoBehaviour {

  public ReactiveProperty<GameScene> SceneName = new ReactiveProperty<GameScene>();

  protected virtual void Awake () {
    var btn = GetComponent<Button>();
    btn.OnClickAsObservable()
      .Subscribe(_ =>
        {
          SceneLoader.Instance.LoadScene(SceneName.Value);
        })
      .AddTo(this);
  }
  void Start()
  {
    var text = GetComponentInChildren<Text>();
    SceneName
      .SubscribeToText(text);
  }
}
