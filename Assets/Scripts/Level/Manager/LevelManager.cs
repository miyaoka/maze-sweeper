using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LevelManager : SingletonMonoBehaviour<LevelManager>
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
  public ReactiveProperty<float> DangerTimer = new ReactiveProperty<float>();
  public ReactiveProperty<float> DangerTimerMax = new ReactiveProperty<float>(10);
  public ReactiveProperty<float> ExitProgress = new ReactiveProperty<float>();

  public ReactiveProperty<ViewState> CurrentView = new ReactiveProperty<ViewState>();

  public ReactiveProperty<bool> IsSelectedBomb = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsSelectedSensor = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsSelectedMedkit = new ReactiveProperty<bool>();

  LevelConfigParam levelConf = new LevelConfigParam(5, 25, .2f, 3, 120);

  public ReactiveProperty<bool> IsAllDead = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsPassExit = new ReactiveProperty<bool>();

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

    IsPassExit
      .Where(p => p)
      .Subscribe(_ => OnExit())
      .AddTo(this);
    IsAllDead
      .Where(p => p)
      .Subscribe(_ => OnLose())
      .AddTo(this);

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
        DangerTimer.Value += t;
        if (DangerTimer.Value >= DangerTimerMax.Value)
        {
          SurvivorManager.Instance.AddDamageToAll(1);

          DangerTimer.Value %= DangerTimerMax.Value;
        }
      })
      .AddTo(this);

    LevelTimer
      .Select(t => t <= 0)
      .DistinctUntilChanged()
      .Subscribe(isTimeout => setTimeout(isTimeout))
      .AddTo(this);

    ExitProgress =
    PlayerManager.Instance.DestCoords
      .Select(c => (float)c.Y / (float)GraphManager.Instance.graph.MaxCoords.Y)
      .ToReactiveProperty();

  }
  void Start()
  {
    var input = GetComponent<PlayerInput>();
    input
      .ToggleInput
      .Subscribe(_ => CurrentView.Value = CurrentView.Value == ViewState.Map ? ViewState.Normal : ViewState.Map)
      .AddTo(this);

    InitLevel(SectorListManager.Instance.Conf(SectorListManager.Instance.CurrentSector.Value));
    //InitLevel(new LevelConfig(10,3,0, 1));

    CurrentView
      .Subscribe(v =>
      {
        switch (v)
        {
          case ViewState.Normal:
            input.IsMovable.Value = true;
            input.IsScrollable.Value = false;
            input.IsTogglable.Value = true;
            break;
          case ViewState.Map:
            input.IsMovable.Value = false;
            input.IsScrollable.Value = true;
            input.IsTogglable.Value = true;
            break;
          case ViewState.Battle:
            input.IsMovable.Value = false;
            input.IsScrollable.Value = false;
            input.IsTogglable.Value = false;
            break;
        }
      });
  }
  public void ItemBtnSelectClear()
  {
    IsSelectedBomb.Value = false;
    IsSelectedSensor.Value = false;
    IsSelectedMedkit.Value = false;
  }
  void setTimeout(bool timeout)
  {
    DangerTimer.Value = 0;
    if (timeout)
    {
      timerStop();
      AudioManager.TimeoutAlert.Play();
      LevelTimer.Value = 0;
      dangerTimerConnect = dangerTimerUpdate.Connect();
    }
    else if(dangerTimerConnect != null)
    {
      timerResume();
      AudioManager.TimeoutAlert.Stop();
      dangerTimerConnect.Dispose();
    }
  }

  public void RoundConfig()
  {
    hud.SetActive(false);
    MenuManager.Instance.RoundConfigDialog().Open(levelConf,
      (param) =>
      {
        levelConf = param;
        InitLevel(new LevelConfig(param.Col, param.Row, param.EnemyRate, param.MaxEnemyCount));

        skycam.RandomRotate();
        hud.SetActive(true);
      },
      () => {
        hud.SetActive(true);
      }
    );
  }

  public void InitLevel(LevelConfig conf)
  {

    var playerNode = GraphManager.Instance.InitGrid(conf);
    AlertCount.Value = 0;
    CurrentView.Value = ViewState.Map;
    LevelTimer.Value = LevelTimerMax.Value = levelConf.Timer;
    SurvivorManager.Instance.Init();
    timerStop();

    floorText.enabled = startText.enabled = guideText.enabled = false;

    Debug.Log(playerNode.Coords);
    PlayerManager.Instance.InitPlayer(playerNode.Coords);
    var ct = CameraManager.Instance.cameraPivot;
    ct.transform.position = GraphManager.Instance.exitZone.transform.position;

    floorText.enabled = true;

    ct
      .DOLocalMove(GraphManager.Instance.CoordsToVec3(playerNode.Coords), 1.5f)
      .SetEase(Ease.InOutCubic)
      .SetDelay(1f)
      .OnComplete(() =>
      {
        floorText.enabled = false;
        CurrentView.Value = ViewState.Normal;
        timerResume();

        startText.enabled = true;
        Observable.Timer(System.TimeSpan.FromSeconds(1f)).Subscribe(s => {
          startText.enabled = false;

          guideText.enabled = true;
          Observable.Timer(System.TimeSpan.FromSeconds(10f)).Subscribe(g => {
            guideText.enabled = false;
          });
        });
      });
  }

  public void OnExit()
  {
    allTimerStop();

    CurrentView.Value = ViewState.Map;
    var input = GetComponent<PlayerInput>();
    input.IsMovable.Value = false;
    input.IsTogglable.Value = false;

    //      GraphManager.Instance.ShowAllNode();
    MenuManager.Instance.ModalDialog().Open(
      "level cleared!",
      new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            SceneLoader.Instance.LoadScene(GameScene.Camp);
          }),
      }
    );
  }

  public void OnLose()
  {
    allTimerStop();

    var input = GetComponent<PlayerInput>();
    input.IsMovable.Value = false;
    input.IsTogglable.Value = false;

    MenuManager.Instance.ModalDialog().Open(
      "You have died...",
      new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            SceneLoader.Instance.LoadScene(GameScene.Lose);
          }),
      }
    );

  }
  void allTimerStop()
  {
    timerStop();
    AudioManager.TimeoutAlert.Stop();
    if(dangerTimerConnect != null)
    {
      dangerTimerConnect.Dispose();
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