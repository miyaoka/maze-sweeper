using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class GraphModel
{
  int divideMargin = 1;
  float connectRatio = .25f;
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
      return NodeList.Where(n => n.Degree.Value == 1).ToList();
    }
  }
  public List<Node> ShuffledNodeList
  {
    get
    {
      var list = NodeList;
      list.Sort((a, b) => Random.value < .5f ? -1 : 1);
      return list;
    }
  }
  public void Clear()
  {
    foreach(var n in NodeList)
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
    for(var x = -1; x <= 1; x++)
    {
      for(var y = -1; y <= 1; y++)
      {
        var neighbor = GetNode(coords + new IntVector2(x, y));
        if(neighbor != null)
        {
          list.Add(neighbor);
        }
      }
    }
    return list;
  }
  public void CreateMaze(Rect rect)
  {
    var rects = new List<Rect> { rect };
    do
    {
      rects = divideRect(rects);
    }
    while(rects.Count > 0);
  }
  public void RemoveDeadend(float prob)
  {
    var deadends = DeadendNodeList
      .Where(n => Random.value < prob);

    foreach(var n in deadends)
    {
      RemoveNode(n.Coords);
    }
  }
  /*
   * create maze by adding passage edges to rects
   */
  List<Rect> divideRect(List<Rect> rects)
  {
    Rect rect, divRect1, divRect2;
    float area;

    //pickup dividable rect
    do
    {
      if(rects.Count == 0)
      {
        return rects;
      }
      rect = rects[0];
      rects.RemoveAt(0);

      area = rect.width * rect.height;
    } while(area < 2);

    var isVerticalDivide = (int)rect.width == (int)rect.height ? Random.value < .5f : rect.width > rect.height;
    var longSide = Mathf.Max((int)rect.width, (int)rect.height);
    var shortSide = Mathf.Min((int)rect.width, (int)rect.height);

    //min divide span
    var margin = Mathf.Min(Mathf.FloorToInt(longSide * .5f) - 1, divideMargin);
    var divPoint = Random.Range(margin, longSide - 1 - margin);

    //connect divided rects
    connectArea(
      (IntVector2)rect.min + (isVerticalDivide ? new IntVector2(divPoint, 0) : new IntVector2(0, divPoint)),
      shortSide,
      isVerticalDivide
    );

    //divide more if has enough area
    if(area > 2)
    {
      divRect1 = divRect2 = rect;

      divPoint++;
      if(isVerticalDivide)
      {
        divRect1.width = divPoint;
        divRect2.xMin += divPoint;
      }
      else
      {
        divRect1.height = divPoint;
        divRect2.yMin += divPoint;
      }
      rects.Insert(0, divRect1);
      rects.Insert(0, divRect2);
    }

    return rects;
  }
  void connectArea(IntVector2 baseCoords, int lineLength, bool isVerticalDivide)
  {
    //list patchable points
    var connectPointList = new List<int>();
    for(var i = 0; i < lineLength; i++)
    {
      connectPointList.Add(i);
    }

    var connectCount = (float)lineLength * connectRatio;

    //create passage at random points
    while(connectCount-- > 0 && connectPointList.Count > 0)
    {
      var i = Random.Range(0, connectPointList.Count);
      var connectPoint = connectPointList[i];
      connectPointList.RemoveAt(i);

      var connectCoords = isVerticalDivide
        ? new IntVector2(0, connectPoint)
        : new IntVector2(connectPoint, 0);
      var connectDir = isVerticalDivide ? Dirs.East : Dirs.North;
      var sourceCoords = baseCoords + connectCoords;
      var targetCoords = sourceCoords + DirCoords[(int)connectDir];
      graph.CreateEdge(sourceCoords, targetCoords);
    }
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
