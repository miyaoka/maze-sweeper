using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public class GameManager : SingletonMonoBehaviour<GameManager>{
  [SerializeField] GameObject playerPrefab;
  [SerializeField] GameObject dialogPrefab;
  [SerializeField] Transform dialogContainer;
  [SerializeField] Slider slider;


  public ReactiveProperty<int> alertCount = new ReactiveProperty<int>();
  public ReactiveProperty<int> enemyCount = new ReactiveProperty<int>();
  List<CharacterPresenter> players = new List<CharacterPresenter> ();


  GridManager gridManager;

  enum State {Init, EnterLevel, OnLevel, ExitLevel};
  State state = State.Init;
  ReactiveProperty<bool> waitingInput = new ReactiveProperty<bool> (true);
  void Awake ()
  {
    if (this != Instance) {
      Destroy (this);
      return;
    }
    DontDestroyOnLoad (this.gameObject);
  }
  void Start(){
    gridManager = GetComponent<GridManager> ();

    removeDialog ();

    var rects = new List<Rect> ();
    rects.Add(new Rect(new Vector2(0,0), new Vector2(3, 4)));


    var update = this
      .UpdateAsObservable ();

    //move control
    update
      .Where (right => Input.GetKeyDown (KeyCode.Space))
      .Subscribe (_ => rects = gridManager.divideRect(rects))
      .AddTo (this);
    update
      .Where (left => Input.GetKeyDown (KeyCode.A))
      .Subscribe (_ => {
        //      createDialog();
      })
      .AddTo (this);

    Debug.Log ("--start");
    StartCoroutine (gameLoop());
    Debug.Log ("--end");
  }
  public DialogPresenter createDialog(){
    removeDialog ();
    var obj = Instantiate (dialogPrefab);
    obj.transform.SetParent (dialogContainer, false);
    var dp = obj.GetComponent<DialogPresenter> ();
    dp.msg.text = Random.value.ToString ();
    return dp;
  }
  void removeDialog(){
    foreach(Transform t in dialogContainer)
    {
      Destroy(t.gameObject);
    }
  }
  public void restart(){
    gridManager.initGrid ();
  }
  IEnumerator gameLoop(){
    Debug.Log ("enter");
    //    yield return StartCoroutine (enterLevel ());
    Debug.Log ("on");
    yield return StartCoroutine (onLevel ());
    Debug.Log ("exit");
    yield return StartCoroutine (exitLevel ());


    //    StartCoroutine (gameLoop());
    //    yield return null;
  }
  IEnumerator enterLevel(){
    while (waitingInput.Value) {
      yield return null;
    }
    waitingInput.Value = true;
  }
  IEnumerator onLevel(){
    gridManager.initGrid ();
    while (state == State.OnLevel) {
      yield return null;
    }
  }
  IEnumerator exitLevel(){
    //    mc.gameObject.SetActive (false);
    yield return null;
  }
  IEnumerator enterBattle(){
    yield return 1;
  }
  IEnumerator onBattle(){
    yield return 1;
  }
  IEnumerator exitBattle(){
    yield return 1;
  }



}