using UnityEngine;
using System.Collections;
using System;
using UniRx;

public class Edge
{
  public readonly Node SourceNode;
  public readonly Node TargetNode;
  public readonly Vector2 Coords;
  public readonly float Angle;
  public ReactiveProperty<int> Type = new ReactiveProperty<int>();
  public bool HasView = false;
  public Edge(Node sourceNode, Node targetNode)
  {
    this.SourceNode = sourceNode;
    this.TargetNode = targetNode;
    sourceNode.AddEdge(this);
    targetNode.AddEdge(this);
    Coords = (Vector2)(sourceNode.Coords + targetNode.Coords) * .5f;

    Angle = getAngle(sourceNode, targetNode);

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
  public float GetAngleFromNode(Node sourceNode)
  {
    var targetNode = OppositeNode(sourceNode);
    return getAngle(sourceNode, targetNode);
  }
  float getAngle(Node sourceNode, Node targetNode)
  {
    var Vector = targetNode.Coords - sourceNode.Coords;
    return Mathf.Atan2(Vector.Y, Vector.X) * Mathf.Rad2Deg;
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
public class EdgeType
{
  public const int Passage = 1 << 1;
  public const int Locked = 1 << 2;

}
