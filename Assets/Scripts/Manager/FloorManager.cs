using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using DG.Tweening;

public enum ViewStateName { Move, Map, Battle };
public enum GameStateName { Init, EnterLevel, OnLevel, ExitLevel };
public class FloorManager : SingletonMonoBehaviour<FloorManager>
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
  public ReactiveProperty<float> LevelTimer = new ReactiveProperty<float>(10);
  public ReactiveProperty<float> LevelTimerMax = new ReactiveProperty<float>();
  public ReactiveProperty<float> dangerTimer = new ReactiveProperty<float>();
  public ReactiveProperty<float> dangerTimerMax = new ReactiveProperty<float>(10);
  public ReactiveProperty<ViewStateName> ViewState = new ReactiveProperty<ViewStateName>();
  public GameStateName GameState = GameStateName.Init;
  public ReactiveProperty<bool> OnBomb = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnMenu = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsMapView = new ReactiveProperty<bool>();


  LevelConfigParam levelConf = new LevelConfigParam(12, 25, .1f, 3, 120);
  public ReactiveProperty<bool> IsAllDead = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsPassExit = new ReactiveProperty<bool>();
  PlayerManager pm;
  IConnectableObservable<float> timerUpdate;
  System.IDisposable timerConnect;
  IConnectableObservable<float> dangerTimerUpdate;
  System.IDisposable dangerTimerConnect;

  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }

    AlertCount
      .Where(c => c > 0)
      .Subscribe(_ => AudioManager.EnemyDetect.Play())
      .AddTo(this);

    pm = PlayerManager.Instance;

    IsMapView = ViewState
      .Select(s => s == ViewStateName.Map)
      .ToReactiveProperty();

    IsPassExit
      .Where(p => p)
      .Subscribe(_ => OnExit())
      .AddTo(this);
    IsAllDead
      .Where(p => p)
      .Subscribe(_ => OnLose())
      .AddTo(this);

  }
  void Start()
  {
    timerUpdate = Observable
      //      .Interval(System.TimeSpan.FromSeconds(10))
      .EveryFixedUpdate()
      .Select(_ => Time.fixedDeltaTime)
      .Publish();
    dangerTimerUpdate = Observable
      .EveryFixedUpdate()
      .Select(_ => Time.fixedDeltaTime)
      .Publish();

    timerUpdate.Subscribe(t => LevelTimer.Value -= t);
    dangerTimerUpdate
      .Subscribe(t =>
      {
        dangerTimer.Value += t;
        if (dangerTimer.Value >= dangerTimerMax.Value)
        {
          SurvivorManager.Instance.AddDamageToAll(1);

          dangerTimer.Value %= dangerTimerMax.Value;
        }
      })
      .AddTo(this);

    LevelTimer
      .Select(t => t <= 0)
      .DistinctUntilChanged()
      .Subscribe(isTimeout => setTimeout(isTimeout))
      .AddTo(this);


    EnterLevel();

  }
  void setTimeout(bool timeout)
  {
    return;
    dangerTimer.Value = 0;
    if (timeout)
    {
      timerStop();
      AudioManager.TimeoutAlert.Play();
      LevelTimer.Value = 0;
      dangerTimerConnect = dangerTimerUpdate.Connect();
    }
    else
    {
      timerResume();
      AudioManager.TimeoutAlert.Stop();
      dangerTimerConnect.Dispose();
    }
  }

  public void LevelConfig()
  {
    hud.SetActive(false);
    OnMenu.Value = true;
    MenuManager.Instance.LevelConfigDialog().Open(levelConf,
      (param) =>
      {
        levelConf = param;
        GameStateManager.Instance.Restart();

        skycam.RandomRotate();
        OnMenu.Value = false;
      },
      () => {
        OnMenu.Value = false;
      }
    );
  }

  public void EnterLevel()
  {
    var pn = GraphManager.Instance.InitGrid(levelConf);
    AlertCount.Value = 0;
    ViewState.Value = ViewStateName.Map;
    LevelTimer.Value = LevelTimerMax.Value = levelConf.Timer;
    SurvivorManager.Instance.Init();
    timerStop();

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
        GameStateManager.Instance.Next();
        /*
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
        */
      });
  }

  public void OnLevel()
  {
  }
  public void OnExit()
  {
    LevelTimer.Value = Mathf.Max(0, LevelTimer.Value);
    setTimeout(false);
    timerStop();


    ViewState.Value = ViewStateName.Map;
    //      GraphManager.Instance.ShowAllNode();
    MenuManager.Instance.ModalDialog().Open(
      "level cleared!",
      new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
             GameStateManager.Instance.Next();
          }),
      }
    );
  }

  public void OnLose()
  {
    //      GraphManager.Instance.DestroyGrid();
    //      ViewState.Value = ViewStateName.Map;
    MenuManager.Instance.ModalDialog().Open(
      "You have died...",
      new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
             GameStateManager.Instance.Lose();
          }),
      }
    );

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

  public void ToggleMap()
  {
    ViewState.Value = ViewState.Value == ViewStateName.Map ? ViewStateName.Move : ViewStateName.Map;
  }

  public void AddTime()
  {
    AudioManager.Powerup.Play();
    LevelTimer.Value = Mathf.Min(LevelTimer.Value + 30f, LevelTimerMax.Value);
  }
  public void AllDead()
  {
    IsAllDead.Value = true;
  }
  public void onExit()
  {
    IsPassExit.Value = true;
  }
}