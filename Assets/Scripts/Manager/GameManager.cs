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
  public ReactiveProperty<bool> OnMenu = new ReactiveProperty<bool>();


  LevelConfigParam levelConf = new LevelConfigParam(12, 25, .1f, 3);
  bool passExit = false;
  bool isAllDead = false;
  PlayerManager pm;
  IConnectableObservable<long> timerUpdate;
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

    pm = PlayerManager.Instance;
  }
  void Start()
  {
    timerUpdate = Observable
      .Interval(System.TimeSpan.FromSeconds(1))
//      .EveryFixedUpdate()
//      .Select(_ => Time.fixedDeltaTime)
      .Publish();

    timerUpdate.Subscribe(_ => LevelTimer.Value -= 1);

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
    var pn = GraphManager.Instance.InitGrid(levelConf);
    AlertCount.Value = 0;
    ViewState.Value = ViewStateName.Move;
    LevelTimer.Value = LevelTimerMax.Value = 60f * 3f;
    SurvivorManager.Instance.Init();
    isAllDead = false;

    timerConnect = timerUpdate.Connect();
    OnMenu.Value = false;


    Debug.Log(pn.Coords);
    pm.InitPlayer(pn.Coords);

    yield return 0;
  }
  IEnumerator levelConfig()
  {
    hud.SetActive(false);
    OnMenu.Value = true;
    MenuManager.Instance.LevelConfigDialog().Open(levelConf,
      (param) =>
      {
        levelConf = param;
        StartCoroutine(enterLevel());
        skycam.RandomRotate();
        OnMenu.Value = false;
      },
      () => {
        OnMenu.Value = false;
      }
    );
    while (OnMenu.Value)
    {
      yield return null;
    }
    hud.SetActive(true);
  }
  IEnumerator onLevel()
  {
    passExit = false;
    while (!isAllDead && !passExit && LevelTimer.Value > 0)
    {
      yield return null;
    }
  }
  IEnumerator exitLevel()
  {
//    OnMenu.Value = true;
    var onExit = true;
    LevelTimer.Value = Mathf.Max(0, LevelTimer.Value);
    timerConnect.Dispose();

    if (passExit)
    {
      ViewState.Value = ViewStateName.Map;
//      GraphManager.Instance.ShowAllNode();
      MenuManager.Instance.ModalDialog().Open(
        "level cleared!",
        new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            onExit = false;
          }),
        }
      );
    }
    else
    {
      //      GraphManager.Instance.DestroyGrid();
//      ViewState.Value = ViewStateName.Map;
      MenuManager.Instance.ModalDialog().Open(
        "You died...",
        new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            onExit = false;
          }),
        }
      );
    }
    while (onExit)
    {
      yield return null;
    }
  }
  public void timerStop()
  {
    timerConnect.Dispose();
  }
  public void timerResume()
  {
    timerConnect = timerUpdate.Connect();
  }
  public void onExit()
  {
    passExit = true;
  }


  public void ToggleMap()
  {
    ViewState.Value = ViewState.Value == ViewStateName.Map ? ViewStateName.Move : ViewStateName.Map;
  }

  public void AddTime()
  {
    LevelTimer.Value = Mathf.Min(LevelTimer.Value + 30f, LevelTimerMax.Value);
  }
  public void AllDead()
  {
    isAllDead = true;
  }
}