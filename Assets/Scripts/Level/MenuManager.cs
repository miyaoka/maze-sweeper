using UnityEngine;
using System.Collections;

public class MenuManager : SingletonMonoBehaviour<MenuManager>
{
  [SerializeField]
  GameObject modalDialogPrefab;
  [SerializeField]
  GameObject roundConfigDialogPrefab;
  [SerializeField]
  Canvas btnCanvas;
  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
  }
  void Start()
  {

  }
  public LevelConfigDialogPresenter RoundConfigDialog()
  {
    var obj = Instantiate(roundConfigDialogPrefab);
    obj.transform.SetParent(btnCanvas.transform, false);
    return obj.GetComponent<LevelConfigDialogPresenter>();
  }

  public ModalDialogPresenter ModalDialog()
  {
    var obj = Instantiate(modalDialogPrefab);
    obj.transform.SetParent(btnCanvas.transform, false);
    return obj.GetComponent<ModalDialogPresenter>();
  }
}
