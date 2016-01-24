using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;
using DG.Tweening;

public class SurvivorPresenter : MonoBehaviour {

  public Text nameText;
  public Text damageText;
  public Text msgText;
  public GameObject body;
  
  void Awake()
  {
    damageText.gameObject.SetActive(false);
    msgText.gameObject.SetActive(false);
  }
  public void ShowDamage(int damage)
  {
    damageText.text = (-damage).ToString();
    popText(damageText);
  }
  public void ShowMsg(string msg)
  {
    msgText.text = msg;
    popText(msgText);
  }
  void popText(Text t)
  {
    t.transform.localPosition = new Vector3(0, 0, -30);
    t.gameObject.SetActive(true);

    t.transform.DOLocalMoveZ(-100, 1f).SetEase(Ease.OutQuad)
      .OnComplete(() => t.gameObject.SetActive(false));
  }
}
