using UnityEngine.UI;

public class LoadDebugSceneButtonPresenter : LoadSceneButtonPresenter
{
  protected override void Awake()
  {
    base.Awake();
    DontDestroyOnLoad(transform.root.gameObject);
    GetComponentInChildren<Text>().text = sceneName.ToString();
  }
}
