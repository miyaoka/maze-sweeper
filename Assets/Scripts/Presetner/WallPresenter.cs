using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;

public class WallPresenter : MonoBehaviour
{
  [SerializeField]
  Button bombButton;

  void Awake()
  {    
    bombButton
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        GraphManager.Instance.BreachWall(this);
      })
      .AddTo(this);
  }
  private Wall wall;
  public Wall Wall
  {
    set
    {
      this.wall = value;

      wall.OnHere
        .CombineLatest(GameManager.Instance.OnBomb, (l, r) => l && r)
        .Subscribe(b => bombButton.gameObject.SetActive(b))
        .AddTo(this);
    }
    get { return this.wall; }
  }
}
