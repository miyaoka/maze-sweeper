using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public enum ViewState { Move, Map, Battle };
public enum State { Init, EnterLevel, OnLevel, ExitLevel };
public class GameManager : SingletonMonoBehaviour<GameManager>
{
  [SerializeField]
  SkyCameraPresenter skycam;

  [SerializeField]
  GameObject hud;

  public ReactiveProperty<int> alertCount = new ReactiveProperty<int>();
  public ReactiveProperty<int> enemyCount = new ReactiveProperty<int>();
  public ReactiveProperty<float> LevelTimer = new ReactiveProperty<float>();
  public ReactiveProperty<ViewState> viewState = new ReactiveProperty<ViewState>();
  public State state = State.Init;


  int col = 15;
  int row = 30;
  float enemy = .08f;
  bool passExit = false;
  ControlManager cm;
  PlayerManager pm;
  IConnectableObservable<float> timerUpdate;
  System.IDisposable timerConnect;


  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;

    cm = GetComponent<ControlManager>();
    pm = PlayerManager.Instance;
  }
  void Start()
  {
    timerUpdate = Observable
      .EveryFixedUpdate()
      .Select(_ => Time.fixedDeltaTime)
      .Publish();

    timerUpdate.Subscribe(t => LevelTimer.Value -= t);

    viewState
      .Subscribe(v =>
      {
        cm.enabled = viewState.Value == ViewState.Map ? false : true;
      })
      .AddTo(this);

    Debug.Log("--start");
    StartCoroutine(gameLoop());
    Debug.Log("--end");
  }

  public void LevelConfig()
  {
    StartCoroutine(levelConfig());
  }

  IEnumerator gameLoop()
  {
    Debug.Log("enter");
    yield return StartCoroutine(enterLevel());
    Debug.Log("onGame");
    yield return StartCoroutine(onLevel());
    Debug.Log("exit");
    yield return StartCoroutine(exitLevel());

    StartCoroutine(gameLoop());
  }
  IEnumerator enterLevel()
  {
    var pn = GraphManager.Instance.InitGrid(col, row, enemy);
    pm.Health.Value = 3;
    alertCount.Value = 0;
    viewState.Value = ViewState.Move;
    LevelTimer.Value = 60f * 5f;

    timerConnect = timerUpdate.Connect();
      

    Debug.Log(pn.Coords);
    pm.SetPos(pn.Coords);

    yield return 0;
  }
  IEnumerator levelConfig()
  {
    hud.SetActive(false);
    bool onConfig = true;
    cm.enabled = false;
    MenuManager.Instance.LevelConfigDialog().Open(
      new LevelConfigDialogDetail(col, row, enemy),
      (param) =>
      {
        col = param.Col;
        row = param.Row;
        enemy = param.Enemy;
        StartCoroutine(enterLevel());
        skycam.RandomRotate();
        onConfig = false;
      },
      () => { onConfig = false; }
    );
    while (onConfig)
    {
      yield return null;
    }
    cm.enabled = true;
    hud.SetActive(true);
  }
  IEnumerator onLevel()
  {
    passExit = false;
    while (PlayerManager.Instance.Health.Value > 0 && !passExit && LevelTimer.Value > 0)
    {
      yield return null;
    }
  }
  IEnumerator exitLevel()
  {

    cm.enabled = false;
    var isOpen = true;
    LevelTimer.Value = 0;
    timerConnect.Dispose();

    if (passExit)
    {
      MenuManager.Instance.ModalDialog().Open(
        "level cleared!\nyou rescued " + PlayerManager.Instance.Health.Value.ToString() + " survivors.",
        new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            isOpen = false;
            cm.enabled = true;
          }),
        }
      );
    }
    else
    {
//      GraphManager.Instance.DestroyGrid();
      viewState.Value = ViewState.Map;
      MenuManager.Instance.ModalDialog().Open(
        "You are dead...",
        new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            isOpen = false;
            cm.enabled = true;
          }),
        }
      );
    }
    while (isOpen)
    {
      yield return null;
    }
  }
  public void onExit()
  {
    StartCoroutine(onExitC());
  }
  IEnumerator onExitC()
  {
    cm.enabled = false;
    var isOpen = true;
    MenuManager.Instance.ModalDialog().Open(
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
    while (isOpen)
    {
      yield return null;
    }
    cm.enabled = true;
  }

  public void toggleMap()
  {
    viewState.Value = viewState.Value == ViewState.Map ? ViewState.Move : ViewState.Map;
  }
}