using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;
using System.Linq;

public class Node
{
  public readonly IntVector2 Coords;
  public ReactiveProperty<int> EnemyCount = new ReactiveProperty<int>(0);
  public ReactiveProperty<int> AlertCount = new ReactiveProperty<int>(-1);
  public ReactiveProperty<bool> HasView = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasGuideView = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsScanned = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsVisited = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnHere = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnDest = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isExit = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasEnergy = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasRescuee = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasItem = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasFire = new ReactiveProperty<bool>();
  public ReadOnlyReactiveProperty<int> ScannedAlertCount;

  public bool IsRoom;
  public bool IsRoomCenter;
  public IntVector2? RoomRootCoords = null;


  public readonly Edge[] EdgeArray = new Edge[4];

  public ReactiveProperty<int> DangerLevel = new ReactiveProperty<int>();
  CompositeDisposable neighborResources = new CompositeDisposable();

  public List<Edge> EdgeList
  {
    get
    {
      return EdgeArray
        .Where(e => e != null)
        .ToList();
    }
  }

  public Node(IntVector2 coords)
  {
    this.Coords = coords;

    OnHere =
      PlayerManager.Instance
        .CurrentCoords
        .Select(n => n == coords)
        .DistinctUntilChanged()
        .ToReactiveProperty();
    OnDest =
      PlayerManager.Instance
        .DestCoords
        .Select(n => n == coords)
        .DistinctUntilChanged()
        .ToReactiveProperty();

    ScannedAlertCount =
      IsScanned
      .CombineLatest(AlertCount, (s, a) => !s ? -1 : a)
      .ToReadOnlyReactiveProperty();

  }

  public void SetNeighborList(List<Node> nNodeList)
  {
    neighborResources.Clear();
    Observable
      .CombineLatest(
        nNodeList
        .Select(n => n.ScannedAlertCount)
        .ToArray()
      )
     .Subscribe(alertList =>
      {
        var al =
        alertList
          .Where(ac => ac > -1)
          .ToList();

        DangerLevel.Value =
        (al.Count == 0 || al.Min() == 0) 
        ? 0
        : al.Where(ac => ac > 0).Count();
      })
      .AddTo(neighborResources);
  }

  public void AddEdge(Edge e)
  {
    int d = edgeAngleIndex(e);
    EdgeArray[d] = e;
  }

  public void RemoveEdge(Edge e)
  {
    int d = edgeAngleIndex(e);
    EdgeArray[d] = null;
  }

  private int edgeAngleIndex(Edge e)
  {
    //normalize degree to 0 - 3
    return ((int)e.GetAngleFromNode(this) + 360) % 360 / 90;
  }
  public List<Node> ReachableNeighborNodeList()
  {
    var neighborNodes = new List<Node>();
    reachableNeighborNodeList(this, neighborNodes);
    return neighborNodes;
  }

  public void reachableNeighborNodeList(Node startNode, List<Node> seeked)
  {
    if(!isNeighbor(startNode) || seeked.Contains(this) )
    {
      return;
    }

    seeked.Add(this);
    connectedNodeList
      .ForEach(n =>
      {
        n.reachableNeighborNodeList(startNode, seeked);
      });
  }
  List<Node> connectedNodeList
  {
    get {
      return EdgeList
        .Select(e => e.OppositeNode(this))
        .ToList();
    }
  }
  bool isNeighbor(Node node)
  {
    var c = Coords - node.Coords;
    return Mathf.Abs(c.X) <= 1 && Mathf.Abs(c.Y) <= 1;
  }

  public event EventHandler OnDestroy;
  public void destroy()
  {
    if (OnDestroy != null)
    {
      OnDestroy(this, EventArgs.Empty);
    }
  }


}
