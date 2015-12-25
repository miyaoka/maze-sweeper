using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;

public class Node {
  public List<Edge> edgeList = new List<Edge> ();
  public readonly IntVector2 coords;
  public ReactiveProperty<int> degree = new ReactiveProperty<int> ();

  public ReactiveProperty<int> enemyCount = new ReactiveProperty<int> (0);
  public ReactiveProperty<int> alertCount = new ReactiveProperty<int> (0);
  public ReactiveProperty<bool> visited = new ReactiveProperty<bool> ();
  public ReactiveProperty<bool> onHere = new ReactiveProperty<bool> ();
  public ReactiveProperty<bool> onDest = new ReactiveProperty<bool> ();
  public ReactiveProperty<int> contents = new ReactiveProperty<int>();
  public ReactiveProperty<bool> isExit = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> hasItem = new ReactiveProperty<bool>(false);
  public bool hasView = false;


  public Node(IntVector2 coords){
    this.coords = coords;
  }

  public void addEdge(Edge e){
    edgeList.Add (e);
    updateDegree ();
  }
  public void removeEdge(Edge e){
    edgeList.Remove (e);
    updateDegree ();
  }

  void updateDegree ()
  {
    degree.Value = edgeList.Count;
  }

  public event EventHandler OnDestroy;
  public void destroy(){
    if (OnDestroy != null)
    {
      OnDestroy(this, EventArgs.Empty);
    }
    degree.Dispose ();
  }


}
