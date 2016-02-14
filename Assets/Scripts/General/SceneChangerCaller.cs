using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UniRx;

[Prefab("SceneChangerCaller")]
public class SceneChangerCaller : SingletonMonoBehaviour<SceneChangerCaller> {
  void Awake () {
    DontDestroyOnLoad(gameObject);
  }
  void Start(){
    GetComponentInChildren<Button>()
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
          var l = SceneChanger.Instance;
      })
      .AddTo(this);
  }
}
