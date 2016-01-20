using UnityEngine;
using System.Collections;
using UniRx;
public class Survivor
{
  public ReactiveProperty<string> Name = new ReactiveProperty<string>();
  public ReactiveProperty<float> MaxHealth = new ReactiveProperty<float>();
  public ReactiveProperty<float> CurrentHealth = new ReactiveProperty<float>();

  public Survivor(string name, float health)
  {
    Name.Value = name;
    MaxHealth.Value = CurrentHealth.Value = health;
  }

}
