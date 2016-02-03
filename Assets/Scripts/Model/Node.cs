using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;
using System.Linq;

public class Node
{
  public readonly IntVector2 Coords;
  public ReactiveProperty<int> Degree = new ReactiveProperty<int>();
  public ReactiveProperty<int> EnemyCount = new ReactiveProperty<int>(0);
  public ReactiveProperty<int> AlertCount = new ReactiveProperty<int>(0);
  public ReactiveProperty<bool> HasView = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsScanned = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnHere = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnDest = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isExit = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasItem = new ReactiveProperty<bool>(false);
  public ReactiveProperty<bool> HasFire = new ReactiveProperty<bool>(false);

  public readonly Edge[] EdgeArray = new Edge[4];

  public List<Edge> EdgeList
  {
    get
    {
      return EdgeArray
        .Where(e => e != null)
        .ToList<Edge>();
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
  }

  public void AddEdge(Edge e)
  {
    int d = edgeAngleIndex(e);
    EdgeArray[d] = e;

    //    EdgeList.Add(e);
    updateDegree();
  }

  public void RemoveEdge(Edge e)
  {
    int d = edgeAngleIndex(e);
    EdgeArray[d] = null;
    //    EdgeList.Remove(e);
    updateDegree();
  }

  private int edgeAngleIndex(Edge e)
  {
    //normalize degree to 0 - 3
    return ((int)e.GetAngleFromNode(this) + 360) % 360 / 90;
  }

  void updateDegree()
  {
    Degree.Value = EdgeList.Count;
  }

  public event EventHandler OnDestroy;
  public void destroy()
  {
    if(OnDestroy != null)
    {
      OnDestroy(this, EventArgs.Empty);
    }
    Degree.Dispose();
  }


}
