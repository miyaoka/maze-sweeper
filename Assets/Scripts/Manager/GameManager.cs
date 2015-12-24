using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public enum ViewState {Move, Map, Battle};
public enum State {Init, EnterLevel, OnLevel, ExitLevel};
public class GameManager : SingletonMonoBehaviour<GameManager>{
  [SerializeField] GameObject playerPrefab;
  [SerializeField] GameObject dialogPrefab;
  [SerializeField] Transform dialogContainer;
  [SerializeField] SkyCameraPresenter skycam;

  [SerializeField] GameObject scene;
  [SerializeField] GameObject hud;

  public ReactiveProperty<int> alertCount = new ReactiveProperty<int>();
  public ReactiveProperty<int> enemyCount = new ReactiveProperty<int>();
  public ReactiveProperty<ViewState> viewState = new ReactiveProperty<ViewState> ();

  GraphManager gridManager;

  public State state = State.Init;
  ReactiveProperty<bool> waitingInput = new ReactiveProperty<bool> (true);

  int col = 10;
  int row = 10;
  float enemy = .08f;
  bool passExit = false;
  ControlManager cm;

  void Awake ()
  {
    if (this != Instance) {
      Destroy (this);
      return;
    }
    cm = GetComponent<ControlManager> ();
  }
  void Start(){
    gridManager = GetComponent<GraphManager> ();

    viewState
      .Subscribe (v => {
          cm.enabled = viewState.Value == ViewState.Map ? false : true;
      })
      .AddTo (this);

    Debug.Log ("--start");
    StartCoroutine (gameLoop());
    Debug.Log ("--end");
  }

  void removeDialog(){
    foreach(Transform t in dialogContainer)
    {
      Destroy(t.gameObject);
    }
  }
  public void restart(){
    StartCoroutine (levelConfig ());
  }

  IEnumerator gameLoop(){
    Debug.Log ("enter");
    yield return StartCoroutine (enterLevel ());
    Debug.Log ("onGame");
    yield return StartCoroutine (onLevel ());
    Debug.Log ("exit");
    yield return StartCoroutine (exitLevel ());

    StartCoroutine (gameLoop());
  }
  IEnumerator enterLevel(){
    PlayerManager.Instance.health.Value = 5;
    GraphManager.Instance.initGrid (col, row, enemy);
    yield return 0;
  }
  IEnumerator levelConfig(){
    hud.SetActive (false);
    bool onConfig = true;
    cm.enabled = false;
    MenuManager.Instance.levelConfigDialog ().open (
      new LevelConfigDialogDetail (col, row, enemy),
      (param) => {
        col = param.col;
        row = param.row;
        enemy = param.enemy;
        StartCoroutine (enterLevel());
        skycam.randomRotate ();
        onConfig = false;
      },
      () => { onConfig = false; }
    );
    while (onConfig) {
      yield return null;
    }
    cm.enabled = true;
    hud.SetActive (true);
  }
  IEnumerator onLevel(){
    passExit = false;
    while (PlayerManager.Instance.health.Value > 0 && !passExit) {
      yield return null;
    }
  }
  IEnumerator exitLevel(){
    
    cm.enabled = false;
    var isOpen = true;

    if (PlayerManager.Instance.health.Value > 0) {
      MenuManager.Instance.modalDialog ().open (
        "level cleared!\nyou rescued " + PlayerManager.Instance.health.Value.ToString() + " survivors.", 
        new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            isOpen = false;
            cm.enabled = true;
          }),
        }
      );      
    } else {
      MenuManager.Instance.modalDialog ().open (
        "You are dead...", 
        new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            isOpen = false;
            cm.enabled = true;
          }),
        }
      );
    }
    while (isOpen) {
      yield return null;
    }
  }
  public void onExit(){
    StartCoroutine (onExitC());
  }
  IEnumerator onExitC(){
    cm.enabled = false;
    var isOpen = true;
    MenuManager.Instance.modalDialog().open(
      "[Exit]\nGo to next level?", 
      new List<DialogOptionDetails>{
        new DialogOptionDetails("Yes", ()=> {
          passExit = true;
          isOpen = false;
        }),
        new DialogOptionDetails("no", ()=> {
          isOpen = false;
        }),
      }
    );
    while (isOpen) {
      yield return null;
    }
    cm.enabled = true;
  }

  public void toggleMap(){
    viewState.Value = viewState.Value == ViewState.Map ? ViewState.Move : ViewState.Map;
  }




}