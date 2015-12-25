using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

[RequireComponent (typeof (Button))]
public class ConfPresenter : MonoBehaviour {
  [SerializeField] Button btn;
  void Start () {

    btn
      .OnClickAsObservable()
      .Subscribe (b => {
        GameManager.Instance.Restart();
      })
      .AddTo(this); 

  }
}