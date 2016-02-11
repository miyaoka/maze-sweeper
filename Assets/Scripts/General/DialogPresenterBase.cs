using UnityEngine;
using UnityEngine.UI;

public class DialogPresenterBase : MonoBehaviour
{
  protected virtual void closePanel()
  {
    var btns = GetComponentsInChildren<Button>();
    foreach(var btn in btns)
    {
      btn.onClick.RemoveAllListeners();
    }
    Destroy(gameObject);
  }
}
