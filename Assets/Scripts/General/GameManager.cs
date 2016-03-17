using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


[Prefab("GameManager", true)]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
  public List<Survivor> SurvivorList = new List<Survivor>();
  public ReactiveProperty<int> BombCount = new ReactiveProperty<int>();
  public ReactiveProperty<int> SensorCount = new ReactiveProperty<int>();
  public ReactiveProperty<int> MedkitCount = new ReactiveProperty<int>();
  public ReactiveProperty<int> GrenadeCount = new ReactiveProperty<int>();

  void Awake()
  {
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;

    Init();
  }

  public void Init()
  {
    SectorListManager.Instance.Init();
    var s = SceneChangerCaller.Instance;

    BombCount.Value = 3;
    SensorCount.Value = 50;
    MedkitCount.Value = 3;
    GrenadeCount.Value = 10;
  }

  public List<Survivor> LivingList
  {
    get
    {
      return SurvivorList
          .Where(s => s.CurrentHealth.Value > 0)
          .ToList();
    }
  }
}
