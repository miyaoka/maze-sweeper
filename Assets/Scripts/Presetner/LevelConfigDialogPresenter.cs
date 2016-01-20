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
  [SerializeField]
  Button showAllBtn;

  UnityAction onClose;
  void Awake()
  {
    panel.SetActive(false);
    rowSlider.maxValue = 200;
    colSlider.minValue = rowSlider.minValue = 8;
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
  public void Open(LevelConfigDialogDetail param, UnityAction<LevelConfigDialogDetail> onSubmit, UnityAction onClose = null)
  {
    this.onClose = onClose;
    colSlider.value = param.Col;
    rowSlider.value = param.Row;
    enemySlider.value = param.Enemy;
    panel.SetActive(true);

    submitBtn.onClick.AddListener(() => onSubmit(new LevelConfigDialogDetail((int)colSlider.value, (int)rowSlider.value, enemySlider.value)));
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
