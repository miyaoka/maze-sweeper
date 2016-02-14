using UnityEngine;
using System.Collections;

public class TitleSceneManager : MonoBehaviour {

  // Use this for initialization
  void Start () {
    GameManager.Instance.Init();
    BGMManager.Instance.Play("In the Rain");
  }

}
