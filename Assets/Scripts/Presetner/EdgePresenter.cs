using DG.Tweening;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class EdgePresenter : MonoBehaviour
{
  [SerializeField]
  GameObject[] walls = new GameObject[2];
  [SerializeField]
  GameObject floor;

  CompositeDisposable typeResources = new CompositeDisposable();
  CompositeDisposable modelResources = new CompositeDisposable();
  Tweener t;

  private Edge edge;
  public Edge Edge
  {
    set
    {
      this.edge = value;

      modelResources.Clear();

      //change image by edgetype
      /*
      model.type
        .Where(t => t != null)
        .Subscribe(t => {
          typeResources.Clear();
          t.isPassable
            .Subscribe(p => edgeImage.gameObject.SetActive(p))
            .AddTo(typeResources);
        })
        .AddTo(this); 
        */

      var mts = walls.Select(w => w.GetComponent<Renderer>().material).ToList();
      mts.ForEach(m => m.DOFade(0, 0));
      //player is on the one of nodes
      edge.SourceNode.OnDest
        .CombineLatest(edge.TargetNode.OnDest, (l, r) => l || r)
        .Subscribe(b =>
        {
          mts.ForEach(m => m.DOFade(b ? .5f : 0, .2f).SetEase(Ease.OutQuad));
         })
        .AddTo(this);

      //visited one of nodes
      edge.SourceNode.IsVisited
        .CombineLatest(edge.TargetNode.IsVisited, (l, r) => l || r)
        .Subscribe(b =>
        {

        })
        .AddTo(this);

      edge.OnDestroy += modelDestoryHandler;
    }
    get { return this.edge; }
  }
  void modelDestoryHandler(object sender, EventArgs e)
  {
    Destroy(gameObject);
  }

  void OnDestroy()
  {
    if(edge != null)
    {
      edge.OnDestroy -= modelDestoryHandler;
    }
    typeResources.Dispose();
  }
}
