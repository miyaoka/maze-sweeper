using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
public class DialogOptionDetails{
  public string title;
  public UnityAction action;
  public DialogOptionDetails(string title, UnityAction action){
    this.title = title;
    this.action = action;
  }
}
public class ModalDialogPresenter : DialogPresenterBase {
  [SerializeField] GameObject optionBtnPrefab;
  [SerializeField] Text msgText;
  [SerializeField] Button outOfPanelBtn;
  [SerializeField] GameObject containerH;
  [SerializeField] GameObject containerV;
  [SerializeField] GameObject panel;

  void Awake () {
    panel.SetActive (false);
    containerH.SetActive (false);
    containerV.SetActive (false);
    msgText.text = "";
  }

  public void open(string msg, List<DialogOptionDetails> options, UnityAction abortAction = null){
    msgText.text = msg;

    panel.SetActive (true);

    var container = (Camera.main.aspect > 1) ? containerH : containerV;
    container.SetActive (true);

    foreach (var option in options) {
      var obj = Instantiate (optionBtnPrefab);
      obj.GetComponentInChildren<Text> ().text = option.title;
      var btn = obj.GetComponent<Button> ();
      btn.onClick.AddListener (option.action);
      btn.onClick.AddListener (closePanel);
      obj.transform.SetParent (container.transform);
    }

    if (abortAction != null) {
      outOfPanelBtn.onClick.AddListener (abortAction);
      outOfPanelBtn.onClick.AddListener (closePanel);
    }
  }
}
