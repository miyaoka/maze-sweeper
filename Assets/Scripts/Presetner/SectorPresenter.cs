using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class SectorPresenter : MonoBehaviour
{
  Sector sector;

  LayoutElement layout;
  Image image;
  Button btn;

//  float heightUnit = 60;

  [SerializeField]
  Text typeText;
  [SerializeField]
  Text sosText;
  [SerializeField]
  Image highlight;
  [SerializeField]
  Image fade;

  void Awake()
  {
    layout = GetComponent<LayoutElement>();
    image = GetComponent<Image>();
    btn = GetComponent<Button>();
  }
  public Sector Sector
  {
    set
    {
      sector = value;

      layout.preferredHeight *= sector.FloorSize;
      image.color = sector.Type.Color;
      sosText.enabled = sector.hasSurvivor;
      typeText.text = sector.Type.Name;

      btn
        .OnClickAsObservable()
        .Subscribe(_ =>
        {
          LevelListManager.Instance.CurrentSector.Value = sector;
          sector.IsVisited = true;
          GameStateManager.Instance.Next();

        })
        .AddTo(this);

      var state =
      LevelListManager.Instance.CurrentSector
        .Select(s => sector.State(s))
        .ToReactiveProperty();

      state
        .Subscribe(s =>
        {
          highlight.enabled = s != SectorState.None;
          highlight.color = SectorState.Next == s ? Color.white
          : SectorState.Current == s ? Color.yellow : Color.clear;

          fade.enabled = SectorState.None == s;
        })
        .AddTo(this);

      state
        .Select(s => s == SectorState.Next)
        .Subscribe(b => btn.enabled = b)
        .AddTo(this);

      //SOS
      var seq = DOTween.Sequence();
      seq
        .Append(sosText.DOFade(1, .5f).SetEase(Ease.InCubic))
        .Append(sosText.DOFade(0, .5f).SetEase(Ease.InCubic))
        .SetLoops(-1);
      seq.Pause();

      if (sosText.enabled)
      {
        seq.Play();
      }
    }
  }
}
