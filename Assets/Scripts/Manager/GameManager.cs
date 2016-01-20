using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public enum ViewStateName { Move, Map, Battle };
public enum GameStateName { Init, EnterLevel, OnLevel, ExitLevel };
public class GameManager : SingletonMonoBehaviour<GameManager>
{
  [SerializeField]
  SkyCameraPresenter skycam;

  [SerializeField]
  GameObject hud;

  public ReactiveProperty<int> AlertCount = new ReactiveProperty<int>();
  public ReactiveProperty<float> LevelTimer = new ReactiveProperty<float>();
  public ReactiveProperty<float> LevelTimerMax = new ReactiveProperty<float>();
  public ReactiveProperty<ViewStateName> ViewState = new ReactiveProperty<ViewStateName>();
  public GameStateName GameState = GameStateName.Init;
  public ReactiveProperty<bool> OnBomb = new ReactiveProperty<bool>();

  int col = 12;
  int row = 25;
  float enemy = .1f;
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

    AlertCount
      .Where(c => c > 0)
      .Subscribe(_ => AudioManager.EnemyDetect.Play())
      .AddTo(this);

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

    ViewState
      .Subscribe(v =>
      {
        cm.enabled = ViewState.Value == ViewStateName.Map ? false : true;
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
    AlertCount.Value = 0;
    ViewState.Value = ViewStateName.Move;
    LevelTimer.Value = LevelTimerMax.Value = 60f * 3f;

    timerConnect = timerUpdate.Connect();
      

    Debug.Log(pn.Coords);
    pm.MovePos(pn.Coords, true);

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
    LevelTimer.Value = Mathf.Max(0, LevelTimer.Value);
    timerConnect.Dispose();

    if (passExit)
    {
      MenuManager.Instance.ModalDialog().Open(
        "level cleared!",
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
      ViewState.Value = ViewStateName.Map;
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
    passExit = true;
//    StartCoroutine(onExitC());
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

  public void ToggleMap()
  {
    ViewState.Value = ViewState.Value == ViewStateName.Map ? ViewStateName.Move : ViewStateName.Map;
  }

  public void AddTime()
  {
    LevelTimer.Value = Mathf.Min(LevelTimer.Value + 30f, LevelTimerMax.Value);
  }
}