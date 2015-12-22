using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogPresenterBase : MonoBehaviour {
  protected void closePanel(){
    var btns = GetComponentsInChildren<Button> ();
    foreach (var btn in btns) {
      btn.onClick.RemoveAllListeners ();
    }
    Destroy (gameObject);
  }
}
