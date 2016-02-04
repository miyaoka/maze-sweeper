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
        GameStateManager.Instance.BombCount.Value -= 1;
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
        .CombineLatest(RoundManager.Instance.IsSelectedBomb, (l, r) => l && r)
        .Subscribe(b => bombButton.gameObject.SetActive(b))
        .AddTo(this);
    }
    get { return this.wall; }
  }
}
