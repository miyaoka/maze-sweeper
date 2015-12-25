using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;

public class Node
{
  public List<Edge> EdgeList = new List<Edge>();
  public readonly IntVector2 Coords;
  public ReactiveProperty<int> Degree = new ReactiveProperty<int>();

  public ReactiveProperty<int> EnemyCount = new ReactiveProperty<int>(0);
  public ReactiveProperty<int> AlertCount = new ReactiveProperty<int>(0);
  public ReactiveProperty<bool> IsVisited = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnHere = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnDest = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isExit = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasItem = new ReactiveProperty<bool>(false);
  public bool HasView = false;


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
    EdgeList.Add(e);
    updateDegree();
  }
  public void RemoveEdge(Edge e)
  {
    EdgeList.Remove(e);
    updateDegree();
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
