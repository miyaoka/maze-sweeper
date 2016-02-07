using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


[Prefab("GameManager")]
public class GameManager : SingletonMonoBehaviour<GameManager>
{
  public List<Survivor> SurvivorList = new List<Survivor>();
  public ReactiveProperty<int> BombCount = new ReactiveProperty<int>(3);
  public ReactiveProperty<int> SensorCount = new ReactiveProperty<int>(10);
  public ReactiveProperty<int> MedkitCount = new ReactiveProperty<int>(3);

  void Awake()
  {
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;

    SectorListManager.Instance.Init();
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


  public void NextScene()
  {
  }
  public void TitleScene()
  {
  }
}
