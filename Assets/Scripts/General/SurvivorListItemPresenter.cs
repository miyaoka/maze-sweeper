﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class SurvivorListItemPresenter : MonoBehaviour
{
  [SerializeField]
  Text nameText;
  [SerializeField]
  LayoutElement healthLayout;
  [SerializeField]
  Image healthImage;
  [SerializeField]
  Image healthDiffImage;
  [SerializeField]
  Text healthText;
  [SerializeField]
  Button medkitTargetBtn;
  [SerializeField]
  Image avatarImage;
  float healthUnitWidth = 20f;

  Survivor survivor;

  void Start()
  {
    
  }
  public Survivor Survivor
  {
    set
    {
      survivor = value;

      survivor
        .Name
        .SubscribeToText(nameText)
        .AddTo(this);

      survivor
        .MaxHealth
        .Subscribe(h => healthLayout.preferredWidth = h * healthUnitWidth)
        .AddTo(this);

      Tweener healthTween = null;

      var healthAmount = 
      survivor
        .CurrentHealth
        .CombineLatest(survivor.MaxHealth, (c, m) => Mathf.Clamp01(c / m))
        .ToReactiveProperty();

      healthImage.fillAmount = healthAmount.Value;

      healthAmount
        .Subscribe(v => {
          if(healthTween != null)
          {
            healthTween.Kill();
          }
          healthTween = healthImage.DOFillAmount(v, 1f);
          /*
          if (v.d > 0)
          {
            tw = healthDiffImage.DOFillAmount(v.v, 1f).OnComplete(() => healthImage.fillAmount = v.v);
          }
          else {
            tw = healthImage.DOFillAmount(v.v, 1f).OnComplete(() => healthDiffImage.fillAmount = v.v);
          }
          */
          
          })
        .AddTo(this);

      survivor
        .CurrentHealth
        .CombineLatest(survivor.MaxHealth, (c, m) => string.Format("{0:F0}/{1:F0}", c, m))
        .SubscribeToText(healthText)
        .AddTo(this);

      var isAlive = 
      survivor
        .CurrentHealth
        .Select(h => h > 0)
        .DistinctUntilChanged()
        .ToReactiveProperty();

      //healable and selected medkit
      healthAmount
        .Select(a => a > 0 && a < 1)
        .CombineLatest(LevelManager.Instance.IsSelectedMedkit, (l, m) => l && m)
        .Subscribe(b => medkitTargetBtn.gameObject.SetActive(b))
        .AddTo(this);

      isAlive
        .Subscribe(l => avatarImage.color = l ? Color.white : Color.red)
        .AddTo(this);

      medkitTargetBtn
        .OnClickAsObservable()
        .Subscribe(_ =>
        {
          AudioManager.Instance.Play(AudioName.Powerup);
          survivor.Heal();
          GameManager.Instance.MedkitCount.Value -= 1;
          LevelManager.Instance.IsSelectedMedkit.Value = false;
        })
        .AddTo(this);
    }
  }

}
