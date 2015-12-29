using UnityEngine;
using System.Collections;
using UniRx;

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

  CompositeDisposable edgeResources = new CompositeDisposable();

  private Node node;
  public uint index;
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
