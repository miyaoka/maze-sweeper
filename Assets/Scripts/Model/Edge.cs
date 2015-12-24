using UnityEngine;
using System.Collections;
using System;
using UniRx;

public class Edge{
  public readonly Node sourceNode;
  public readonly Node targetNode;
  public readonly Dirs dir;
  public readonly Vector2 coords;
  public readonly IntVector2 vector;
  public ReactiveProperty<EdgeType> type = new ReactiveProperty<EdgeType> (EdgeType.passage);
  public bool hasView = false;

  public Edge(Node sourceNode, Node targetNode, Dirs dir){
    this.sourceNode = sourceNode;
    this.targetNode = targetNode;
    this.dir = dir;
    sourceNode.addEdge (this);
    targetNode.addEdge (this);
    vector = targetNode.coords - sourceNode.coords;
    coords = (Vector2)(sourceNode.coords + targetNode.coords) * .5f;

    sourceNode.OnDestroy += nodeDestoryHandler;
//    targetNode.OnDestroy += nodeDestoryHandler;
  }
  //undirected node
  public Node oppositeNode(Node node){
    if (node == sourceNode) {
      return targetNode;
    } else if (node == targetNode) {
      return sourceNode;
    } else {
      throw new UnityException ("Illegal node");
    }
  }
  void nodeDestoryHandler (object sender, EventArgs e)
  {
    destroy ();
  }
  public event EventHandler OnDestroy;
  public void destroy(){
    OnDestroy (this, EventArgs.Empty);
    sourceNode.removeEdge (this);
    targetNode.removeEdge (this);
    sourceNode.OnDestroy -= nodeDestoryHandler;
    targetNode.OnDestroy -= nodeDestoryHandler;
  }
}
