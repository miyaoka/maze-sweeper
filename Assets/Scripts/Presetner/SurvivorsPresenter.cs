using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;

public class SurvivorsPresenter : MonoBehaviour {
  [SerializeField] LayoutElement le;
  void Start () {
    PlayerManager.Instance.health
      .Subscribe (v => le.preferredWidth = v * 16)
      .AddTo (this);  
  }
}
