using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
  public List<Survivor> SurvivorList = new List<Survivor>();
  public ReactiveProperty<int> BombCount = new ReactiveProperty<int>(3);
  public ReactiveProperty<int> SensorCount = new ReactiveProperty<int>(2);
  public ReactiveProperty<int> MedkitCount = new ReactiveProperty<int>(3);

  Animator states;

  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;

    states = GetComponent<Animator>();
    DontDestroyOnLoad(this);
  }
	void Start () {
//    Next();
	
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
  public void Next()
  {
    states.SetTrigger("Next");
  }
  public void Title()
  {
    states.SetTrigger("Title");
  }
  public void Win()
  {
    states.SetTrigger("Win");
  }
  public void Lose()
  {
    states.SetTrigger("Lose");
  }
  public void Restart()
  {
    states.SetTrigger("Restart");
  }
}
