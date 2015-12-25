using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UnityEngine.Events;

public class LevelConfigDialogDetail
{
  public int Col;
  public int Row;
  public float Enemy;
  public LevelConfigDialogDetail(int col, int row, float enemy)
  {
    this.Col = col;
    this.Row = row;
    this.Enemy = enemy;
  }
}
public class LevelConfigDialogPresenter : DialogPresenterBase
{
  [SerializeField]
  Button outOfPanelBtn;
  [SerializeField]
  GameObject panel;
  [SerializeField]
  Slider colSlider;
  [SerializeField]
  Slider rowSlider;
  [SerializeField]
  Slider enemySlider;
  [SerializeField]
  Text colText;
  [SerializeField]
  Text rowText;
  [SerializeField]
  Text enemyText;
  [SerializeField]
  Button submitBtn;
  void Awake()
  {
    panel.SetActive(false);
    rowSlider.maxValue = 200;
  }

  void Start()
  {
    colSlider
      .OnValueChangedAsObservable()
      .Subscribe(v => colText.text = v.ToString())
      .AddTo(this);
    rowSlider
      .OnValueChangedAsObservable()
      .Subscribe(v => rowText.text = v.ToString())
      .AddTo(this);
    enemySlider
      .OnValueChangedAsObservable()
      .Subscribe(v => enemyText.text = v.ToString("P1"))
      .AddTo(this);
  }
  public void Open(LevelConfigDialogDetail param, UnityAction<LevelConfigDialogDetail> submit, UnityAction abort = null)
  {
    colSlider.value = param.Col;
    rowSlider.value = param.Row;
    enemySlider.value = param.Enemy;
    panel.SetActive(true);

    submitBtn.onClick.AddListener(() => submit(new LevelConfigDialogDetail((int)colSlider.value, (int)rowSlider.value, enemySlider.value)));
    submitBtn.onClick.AddListener(closePanel);

    if(abort != null)
    {
      outOfPanelBtn.onClick.AddListener(abort);
      outOfPanelBtn.onClick.AddListener(closePanel);
    }

  }
}
