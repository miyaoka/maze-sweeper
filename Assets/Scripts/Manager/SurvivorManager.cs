using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SurvivorManager : SingletonMonoBehaviour<SurvivorManager>
{
  [SerializeField]
  Transform SurvivorListContainer;
  [SerializeField]
  GameObject SurvivorListItemPrefab;

  public List<Survivor> SurvivorList = new List<Survivor>();

  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }
  }

  public void Init()
  {
    clearView();
    SurvivorList.Clear();

    SurvivorList.Add(new Survivor("Andy", 3));
    SurvivorList.Add(new Survivor("Brian", 3));
    SurvivorList.Add(new Survivor("Charles", 2));
    SurvivorList.Add(new Survivor("David", 2));

    SurvivorList
      .ForEach(s => addView(s));

  }
  public void AddDamages(float totalDamage)
  {
    while (totalDamage > 0)
    {
      var livingList = LivingList;
      if (!livingList.Any())
        return;

      var randomSurvivor = livingList.ElementAt(Random.Range(0, livingList.Count()));
      var damage = Mathf.Min(totalDamage, 1);
      totalDamage -= damage;
      randomSurvivor.CurrentHealth.Value -= damage;
      if(randomSurvivor.CurrentHealth.Value <= 0)
      {
        PlayerManager.Instance.CreateDead(randomSurvivor);
      }
    }
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
  void clearView()
  {
    foreach (Transform t in SurvivorListContainer)
    {
      Destroy(t.gameObject);
    }
  }
  void addView(Survivor s)
  {
    var go = Instantiate(SurvivorListItemPrefab);
    go.GetComponent<SurvivorListItemPresenter>().Survivor = s;
    go.transform.SetParent(SurvivorListContainer, false);
  }
}
