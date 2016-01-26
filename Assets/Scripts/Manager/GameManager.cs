using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using DG.Tweening;

public enum ViewStateName { Move, Map, Battle };
public enum GameStateName { Init, EnterLevel, OnLevel, ExitLevel };
public class GameManager : SingletonMonoBehaviour<GameManager>
{
  [SerializeField]
  SkyCameraPresenter skycam;

  [SerializeField]
  GameObject hud;

  [SerializeField]
  Text floorText;
  [SerializeField]
  Text startText;
  [SerializeField]
  Text guideText;

  public ReactiveProperty<int> AlertCount = new ReactiveProperty<int>();
  public ReactiveProperty<float> LevelTimer = new ReactiveProperty<float>();
  public ReactiveProperty<float> LevelTimerMax = new ReactiveProperty<float>();
  public ReactiveProperty<ViewStateName> ViewState = new ReactiveProperty<ViewStateName>();
  public GameStateName GameState = GameStateName.Init;
  public ReactiveProperty<bool> OnBomb = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnMenu = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsMapView = new ReactiveProperty<bool>();


  LevelConfigParam levelConf = new LevelConfigParam(12, 25, .1f, 3, 180);
  bool passExit = false;
  bool isAllDead = false;
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

    pm = PlayerManager.Instance;

    IsMapView = ViewState
      .Select(s => s == ViewStateName.Map)
      .ToReactiveProperty();
  }
  void Start()
  {
    timerUpdate = Observable
//      .Interval(System.TimeSpan.FromSeconds(10))
      .EveryFixedUpdate()
      .Select(_ => Time.fixedDeltaTime)
      .Publish();

    timerUpdate.Subscribe(t => LevelTimer.Value -= t);

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
    timerStop();
    var pn = GraphManager.Instance.InitGrid(levelConf);
    AlertCount.Value = 0;
    ViewState.Value = ViewStateName.Map;
    LevelTimer.Value = LevelTimerMax.Value = levelConf.Timer;
    SurvivorManager.Instance.Init();
    isAllDead = false;

    floorText.enabled = startText.enabled = guideText.enabled = false;

    Debug.Log(pn.Coords);
    pm.InitPlayer(pn.Coords);
    var ct = CameraManager.Instance.cameraPivot;
    ct.transform.position = GraphManager.Instance.exitZone.transform.position;

    floorText.enabled = true;

    ct
      .DOLocalMove(GraphManager.Instance.CoordsToVec3(pn.Coords), 1.5f)
      .SetEase(Ease.InOutCubic)
      .SetDelay(1f)
      .OnComplete(() =>
      {
        floorText.enabled = false;
        ViewState.Value = ViewStateName.Move;
        timerResume();
        OnMenu.Value = false;

        startText.enabled = true;
        Observable.Timer(System.TimeSpan.FromSeconds(1f)).Subscribe(s => {
          startText.enabled = false;

          guideText.enabled = true;
          Observable.Timer(System.TimeSpan.FromSeconds(10f)).Subscribe(g => {
            guideText.enabled = false;
          });
        });
      });

    while (OnMenu.Value)
    {
      yield return null;
    }
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
        "You have died...",
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
    if(timerConnect != null)
    {
      timerConnect.Dispose();
    }
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