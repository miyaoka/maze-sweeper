using UnityEngine;
using System.Collections;

public class MenuManager : SingletonMonoBehaviour<MenuManager>{

  [SerializeField] GameObject modalDialogPrefab;
  [SerializeField] GameObject levelConfigDialogPrefab;
  [SerializeField] Canvas btnCanvas;
  void Awake ()
  {
    if (this != Instance) {
      Destroy (this);
      return;
    }
  }
  void Start () {

  }
  public LevelConfigDialogPresenter levelConfigDialog(){
    var obj = Instantiate (levelConfigDialogPrefab);
    obj.transform.SetParent (btnCanvas.transform, false);
    return obj.GetComponent<LevelConfigDialogPresenter> ();
  }

  public ModalDialogPresenter modalDialog(){
    var obj = Instantiate (modalDialogPrefab);
    obj.transform.SetParent (btnCanvas.transform, false);
    return obj.GetComponent<ModalDialogPresenter> ();
  }
}
