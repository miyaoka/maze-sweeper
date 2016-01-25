using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx;
using UnityEngine.Events;

public class LevelConfigParam
{
  public int Col;
  public int Row;
  public float EnemyRate;
  public int MaxEnemyCount;
  public LevelConfigParam(int col, int row, float enemyRate, int maxEnemyCount)
  {
    this.Col = col;
    this.Row = row;
    this.EnemyRate = enemyRate;
    this.MaxEnemyCount = maxEnemyCount;
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
  Slider enemyRateSlider;
  [SerializeField]
  Slider enemyCountSlider;
  [SerializeField]
  Text colText;
  [SerializeField]
  Text rowText;
  [SerializeField]
  Text enemyRateText;
  [SerializeField]
  Text enemyCountText;
  [SerializeField]
  Button submitBtn;
  [SerializeField]
  Button showAllBtn;

  UnityAction onClose;
  void Awake()
  {
    panel.SetActive(false);
    colSlider.maxValue = 25;
    rowSlider.maxValue = 100;
    colSlider.minValue = rowSlider.minValue = 8;
    enemyRateSlider.minValue = .07f;
    enemyRateSlider.maxValue = .17f;
    enemyCountSlider.minValue = 1;
    enemyCountSlider.maxValue = 10;
  }

  void Start()
  {
    colSlider
      .OnValueChangedAsObservable()
      .SubscribeToText(colText)
      .AddTo(this);
    rowSlider
      .OnValueChangedAsObservable()
      .SubscribeToText(rowText)
      .AddTo(this);
    enemyRateSlider
      .OnValueChangedAsObservable()
      .Subscribe(v => enemyRateText.text = v.ToString("P1"))
      .AddTo(this);
    enemyCountSlider
      .OnValueChangedAsObservable()
      .SubscribeToText(enemyCountText)
      .AddTo(this);
  }
  public void Open(LevelConfigParam param, UnityAction<LevelConfigParam> onSubmit, UnityAction onClose = null)
  {
    this.onClose = onClose;
    colSlider.value = param.Col;
    rowSlider.value = param.Row;
    enemyRateSlider.value = param.EnemyRate;
    enemyCountSlider.value = param.MaxEnemyCount;
    panel.SetActive(true);

    submitBtn.onClick.AddListener(() => onSubmit(
      new LevelConfigParam((int)colSlider.value, (int)rowSlider.value, enemyRateSlider.value, (int)enemyCountSlider.value)));
    submitBtn.onClick.AddListener(closePanel);

    showAllBtn.onClick.AddListener(() => GraphManager.Instance.ShowAllNode());
    showAllBtn.onClick.AddListener(closePanel);

    outOfPanelBtn.onClick.AddListener(closePanel);
  }
  protected override void closePanel()
  {
    base.closePanel();
    if (onClose != null)
    {
      onClose();
    }
  }
}
