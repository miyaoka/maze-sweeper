using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

[RequireComponent (typeof (Button))]
public class ConfBtnPresenter : MonoBehaviour {
  [SerializeField] Button btn;
  void Start () {

    btn
      .OnClickAsObservable()
      .Subscribe (b => {
        RoundManager.Instance.LevelConfig();
      })
      .AddTo(this); 

  }
}