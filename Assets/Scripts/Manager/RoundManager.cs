using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum ViewStateName { Move, Map, Battle };
public enum GameStateName { Init, EnterLevel, OnLevel, ExitLevel };
public class RoundManager : SingletonMonoBehaviour<RoundManager>
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
  public ReactiveProperty<float> RoundTimer = new ReactiveProperty<float>(10);
  public ReactiveProperty<float> RoundTimerMax = new ReactiveProperty<float>();
  public ReactiveProperty<float> DangerTimer = new ReactiveProperty<float>();
  public ReactiveProperty<float> DangerTimerMax = new ReactiveProperty<float>(10);
  public ReactiveProperty<ViewStateName> ViewState = new ReactiveProperty<ViewStateName>();
  public GameStateName GameState = GameStateName.Init;
  public ReactiveProperty<bool> IsSelectedBomb = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsSelectedSensor = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsSelectedMedkit = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnMenu = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsMapView = new ReactiveProperty<bool>();
  public ReactiveProperty<float> ExitProgress = new ReactiveProperty<float>();

  RoundConfigParam roundConf = new RoundConfigParam(12, 25, .1f, 3, 120);
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

    timerUpdate.Subscribe(t => RoundTimer.Value -= t);
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

    RoundTimer
      .Select(t => t <= 0)
      .DistinctUntilChanged()
      .Subscribe(isTimeout => setTimeout(isTimeout))
      .AddTo(this);

    ExitProgress =
    PlayerManager.Instance.DestCoords
      .Select(c => (float)c.Y / (float)GraphManager.Instance.graph.MaxCoords.Y)
      .ToReactiveProperty();

    InitRound(SectorListManager.Instance.Conf(SectorListManager.Instance.CurrentSector.Value));

    pm.DestCoords
      .CombineLatest(pm.CurrentCoords, (l, r) => l != r)
      .Where(moving => moving)
      .Subscribe(_ =>
      {
        ItemBtnSelectClear();
      })
      .AddTo(this);

  }
  public void ItemBtnSelectClear()
  {
    IsSelectedBomb.Value = false;
    IsSelectedSensor.Value = false;
    IsSelectedMedkit.Value = false;
  }
  void setTimeout(bool timeout)
  {
    return;
    DangerTimer.Value = 0;
    if (timeout)
    {
      timerStop();
      AudioManager.TimeoutAlert.Play();
      RoundTimer.Value = 0;
      dangerTimerConnect = dangerTimerUpdate.Connect();
    }
    else
    {
      timerResume();
      AudioManager.TimeoutAlert.Stop();
      dangerTimerConnect.Dispose();
    }
  }

  public void RoundConfig()
  {
    hud.SetActive(false);
    MenuManager.Instance.RoundConfigDialog().Open(roundConf,
      (param) =>
      {
        roundConf = param;
        InitRound(new RoundConfig(param.Col, param.Row, param.EnemyRate, param.MaxEnemyCount));
//        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        //        GameStateManager.Instance.Restart();

        skycam.RandomRotate();
        hud.SetActive(true);
      },
      () => {
        hud.SetActive(true);
      }
    );
  }

  public void InitRound(RoundConfig conf)
  {

    var playerNode = GraphManager.Instance.InitGrid(conf);
    AlertCount.Value = 0;
    ViewState.Value = ViewStateName.Map;
    RoundTimer.Value = RoundTimerMax.Value = roundConf.Timer;
    SurvivorManager.Instance.Init();
    timerStop();

    floorText.enabled = startText.enabled = guideText.enabled = false;

    Debug.Log(playerNode.Coords);
    pm.InitPlayer(playerNode.Coords);
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
  }

  public void OnExit()
  {
    RoundTimer.Value = Mathf.Max(0, RoundTimer.Value);
    setTimeout(false);
    timerStop();


    ViewState.Value = ViewStateName.Map;
    //      GraphManager.Instance.ShowAllNode();
    MenuManager.Instance.ModalDialog().Open(
      "level cleared!",
      new List<DialogOptionDetails> {
          new DialogOptionDetails ("ok", () => {
            SceneLoader.Instance.LoadScene(SceneName.Camp);
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
            SceneLoader.Instance.LoadScene(SceneName.Lose);
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
    RoundTimer.Value = Mathf.Min(RoundTimer.Value + 30f, RoundTimerMax.Value);
  }
  public void AllDead()
  {
    IsAllDead.Value = true;
  }
  public void onExit()
  {
    IsPassExit.Value = true;
  }

  public void UseBomb()
  {

  }
  public void UseSensor()
  {

  }
  public void UseMedkit()
  {

  }
}