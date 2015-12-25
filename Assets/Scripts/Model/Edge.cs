using UnityEngine;
using System.Collections;
using System;
using UniRx;

public class Edge
{
  public readonly Node SourceNode;
  public readonly Node TargetNode;
  public readonly Vector2 Coords;
  public readonly IntVector2 Vector;
  public readonly float Deg;
  public ReactiveProperty<EdgeType> Type = new ReactiveProperty<EdgeType>(EdgeType.passage);
  public bool HasView = false;
  public Edge(Node sourceNode, Node targetNode)
  {
    this.SourceNode = sourceNode;
    this.TargetNode = targetNode;
    sourceNode.AddEdge(this);
    targetNode.AddEdge(this);
    Vector = targetNode.Coords - sourceNode.Coords;
    Coords = (Vector2)(sourceNode.Coords + targetNode.Coords) * .5f;
    Deg = Mathf.Atan2(Vector.Y, Vector.X) * Mathf.Rad2Deg;

    sourceNode.OnDestroy += nodeDestoryHandler;
    targetNode.OnDestroy += nodeDestoryHandler;
  }
  //undirected node
  public Node OppositeNode(Node node)
  {
    if (node == SourceNode)
    {
      return TargetNode;
    }
    else if (node == TargetNode)
    {
      return SourceNode;
    }
    else
    {
      throw new UnityException("Illegal node");
    }
  }
  public event EventHandler OnDestroy;
  public void Destroy(object sender)
  {
    if (OnDestroy != null)
    {
      OnDestroy(sender, EventArgs.Empty);
    }
    SourceNode.RemoveEdge(this);
    TargetNode.RemoveEdge(this);
    SourceNode.OnDestroy -= nodeDestoryHandler;
    TargetNode.OnDestroy -= nodeDestoryHandler;
  }
  void nodeDestoryHandler(object sender, EventArgs e)
  {
    Destroy(sender);
  }
}
