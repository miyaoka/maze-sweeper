using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.UI;

public class WallPresenter : MonoBehaviour
{
  [SerializeField]
  GameObject wholeWall;
  [SerializeField]
  GameObject wallLR;
  [SerializeField]
  GameObject doorL;
  [SerializeField]
  GameObject doorR;
  [SerializeField]
  Button bombButton;
  [SerializeField]
  GameObject explosionPrefab;

  CompositeDisposable edgeResources = new CompositeDisposable();

  private Node node;
  public int index;

  ReactiveProperty<bool> isBreachable = new ReactiveProperty<bool>();

  void Awake()
  {    
    bombButton
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        GameManager.Instance.OnBomb.Value = false;
        GraphManager.Instance.BreachWall(node.Coords, index);
        var obj = Instantiate(explosionPrefab, new Vector3(1, .5f, 0), Quaternion.identity) as GameObject;
        obj.transform.SetParent(this.transform, false);
        Destroy(obj, 3f);
      })
      .AddTo(this);

    isBreachable
      .CombineLatest(GameManager.Instance.OnBomb, (l, r) => l && r)
      .Subscribe(b => bombButton.gameObject.SetActive(b))
      .AddTo(this);
  }
  public Node Node
  {
    set
    {
      this.node = value;
      edgeResources.Dispose();

      node.Degree
        .Subscribe(_ =>
        {
          edgeResources.Clear();

          var edge = node.EdgeArray[index];

          isBreachable.Value = edge == null;
//          && GraphManager.Instance.graph.GetNode(node.Coords + GraphModel.DirCoords[index]) != null;
          if (edge == null)
          {
            wholeWall.SetActive(true);
            wallLR.SetActive(false);
            doorL.SetActive(false);
            doorR.SetActive(false);
            return;
          }
          edge.Type.Subscribe(t =>
          {
            wholeWall.SetActive(false);
            wallLR.SetActive(true);
            doorL.SetActive(true);
            doorR.SetActive(true);
          }).AddTo(edgeResources);

        })
        .AddTo(this);
    }
  }
  void OnDestroy()
  {
    edgeResources.Dispose();
  }
}
