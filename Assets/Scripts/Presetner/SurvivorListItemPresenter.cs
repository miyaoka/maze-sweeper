using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;

public class SurvivorListItemPresenter : MonoBehaviour
{
  [SerializeField]
  Text nameText;
  [SerializeField]
  LayoutElement healthLayout;
  [SerializeField]
  RectTransform healthTransform;
  [SerializeField]
  Text healthText;
  [SerializeField]
  float healthUnitWidth = 10f;

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

      survivor
        .CurrentHealth
        .CombineLatest(survivor.MaxHealth, (c, m) => Mathf.Clamp01(c / m))
        .Subscribe(h => healthTransform.anchorMax = new Vector2(h, 1))
        .AddTo(this);

      survivor
        .CurrentHealth
        .CombineLatest(survivor.MaxHealth, (c, m) => string.Format("{0:F0}/{1:F0}", c, m))
        .SubscribeToText(healthText)
        .AddTo(this);

    }
  }

}
