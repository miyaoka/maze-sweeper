using UnityEngine;
using System.Collections;
using System;

[Prefab("SceneChanger")]
public class SceneChanger : SingletonMonoBehaviour<SceneChanger> {
  [SerializeField]
  GameObject sceneChangeButton;
  [SerializeField]
  Transform buttonContainer;

  void Awake () {
//    DontDestroyOnLoad(gameObject);
  }
  void Start(){
    foreach (Transform t in buttonContainer)
    {
      Destroy(t.gameObject);
    }
    foreach(GameScene s in Enum.GetValues(typeof(GameScene))){
      var obj = Instantiate(sceneChangeButton);
      obj.transform.SetParent(buttonContainer, false);
      obj.GetComponent<LoadDebugSceneButtonPresenter>().SceneName.Value = s;
    }
  }
}
