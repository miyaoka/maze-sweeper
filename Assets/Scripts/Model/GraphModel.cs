using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class GraphModel
{
  public GraphModel()
  {
  }
  Dictionary<IntVector2, Node> nodeDict = new Dictionary<IntVector2, Node>();

  public Node CreateNode(IntVector2 coords, bool ifNotExist = false)
  {
    if(ifNotExist && nodeDict.ContainsKey(coords))
    {
      return nodeDict[coords];
    }
    var n = new Node(coords);
    nodeDict.Add(coords, n);
    return n;
  }
  public Edge CreateEdge(IntVector2 coords0, IntVector2 coords1)
  {
    var n0 = CreateNode(coords0, true);
    var n1 = CreateNode(coords1, true);
    var edge = new Edge(n0, n1);
    return edge;
  }
  public Node GetNode(IntVector2 coords)
  {
    return nodeDict.ContainsKey(coords) ? nodeDict[coords] : null;
  }
  public void RemoveNode(IntVector2 coords)
  {
    var n = GetNode(coords);
    var list = new List<Edge>(n.EdgeList);
    foreach(var e in list)
    {
      n.RemoveEdge(e);
      e.OppositeNode(n).RemoveEdge(e);
    }
    nodeDict.Remove(coords);
  }

  public int NodeCount
  {
    get
    {
      return nodeDict.Count;
    }
  }
  public List<Node> NodeList
  {
    get
    {
      return nodeDict.Values.ToList();
    }
  }
  public List<Node> DeadendNodeList
  {
    get
    {
      return new List<Node>(nodeDict.Values).Where(n => n.Degree.Value == 1).ToList();
    }
  }
  public List<Node> ShuffledNodeList
  {
    get
    {
      var list = new List<Node>(nodeDict.Values);
      list.Sort((a, b) => Random.value < .5f ? -1 : 1);
      return list;
    }
  }
  public void Clear()
  {
    var nodes = nodeDict.Values.ToArray<Node>();
    foreach(var n in nodes)
    {
      n.destroy();
      RemoveNode(n.Coords);
    }
    nodeDict.Clear();
  }

  public int ScanEnemies(IntVector2 coords)
  {
    var count = 0;
    foreach(var n in Neighbors(coords))
    {
      count += n.EnemyCount.Value;
    }
    return count;
  }
  public List<Node> Neighbors(IntVector2 coords)
  {
    var list = new List<Node>();
    for(var nx = coords.X - 1; nx <= coords.X + 1; nx++)
    {
      for(var ny = coords.Y - 1; ny <= coords.Y + 1; ny++)
      {
        var dest = new IntVector2(nx, ny);
        if(dest == coords)
        {
          continue;
        }

        var neighbor = GetNode(dest);
        if(neighbor != null)
        {
          list.Add(neighbor);
        }
      }
    }
    return list;
  }
}
public class EdgeType
{

  public const int DIR_TOP = 0;
  public const int DIR_RIGHT = 1;
  public ReactiveProperty<bool> isPassable = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isBreachable = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isBreached = new ReactiveProperty<bool>();
  public ReactiveProperty<int> doorCount = new ReactiveProperty<int>();

  public EdgeType(bool passable, bool breachable, int doors, bool breached = false)
  {
    isPassable.Value = passable;
    isBreachable.Value = breachable;
    doorCount.Value = doors;
    isBreached.Value = breached;
  }
  public static EdgeType unbreachableWall { get { return new EdgeType(false, false, 0); } }
  public static EdgeType wall { get { return new EdgeType(false, true, 0); } }
  public static EdgeType passage { get { return new EdgeType(true, false, 0); } }
  public static EdgeType door(int doors)
  {
    return new EdgeType(false, true, doors);
  }
  public EdgeType breach()
  {
    if(!isBreachable.Value)
    {
      return this;
    }
    doorCount.Value = 0;
    isPassable.Value = true;
    isBreached.Value = true;
    return this;
  }
}
