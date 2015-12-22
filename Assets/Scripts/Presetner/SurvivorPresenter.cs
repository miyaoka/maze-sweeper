using UnityEngine;
using System.Collections;

public class SurvivorPresenter : MonoBehaviour {
  void Start () {
    var am = GetComponentInChildren<Animator> ();
    am.SetBool("isWalking", true);
  }
}
