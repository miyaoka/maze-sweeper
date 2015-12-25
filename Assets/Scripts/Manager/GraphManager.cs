using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public enum Dirs { North, East, South, West, Null };
public class GraphManager : SingletonMonoBehaviour<GraphManager>
{
  [SerializeField]
  Transform viewContainer;
  [SerializeField]
  GameObject gridNode3DPrefab;
  [SerializeField]
  GameObject gridEdge3DPrefab;
  [SerializeField]
  public float gridUnit = 320;
  [SerializeField]
  float deadEndReduceProb = .5f;
  [SerializeField]
  float itemProb = .3f;

  public static readonly IntVector2[] DirCoords = { new IntVector2(0, 1), new IntVector2(1, 0), new IntVector2(0, -1), new IntVector2(-1, 0) };
  int divideMargin = 1;
  float connectPerLength = .25f;
  public readonly GraphModel graph = new GraphModel();
  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    foreach(Transform t in viewContainer)
    {
      Destroy(t.gameObject);
    }
  }
  void Start()
  {

  }
  public void InitGrid(int gridWidth, int gridHeight, float enemyRatio, UnityAction callback)
  {
    graph.Clear();

    createMaze(new Rect(new Vector2(0, 0), new Vector2(gridWidth, gridHeight)));

    deadEndReduceProb = .5f;
    var count = 2;
    while(count-- > 0)
    {
      removeDeadend(deadEndReduceProb);
    }
    var list = graph.ShuffledNodeList;
    Debug.Log(list.Count);
    addEnemies(list, enemyRatio, 2);
    Debug.Log(list.Count);
    createExit();
    createExit();
    createExit();
    addItems();

    setPlayerPos(list);


    var t = System.DateTime.Now;
    foreach(var n in graph.NodeList)
    {
      addNodeView(n);
      n.AlertCount.Value = graph.ScanEnemies(n.Coords);
      foreach(var e in n.EdgeList)
      {
        addEdgeView(e);
      }
    }
    var t2 = System.DateTime.Now;
    Debug.Log("Instantiate: " + (t2 - t));
  }

  void createMaze(Rect rect)
  {
    var rects = new List<Rect> { rect };
    do
    {
      rects = divideRect(rects);
    }
    while(rects.Count > 0);
  }

  void removeDeadend(float prob)
  {
    var deadends = graph
      .DeadendNodeList
      .Where(n => Random.value < prob);

    foreach(var n in deadends)
    {
      graph.RemoveNode(n.Coords);
    }
  }

  List<Node> addEnemies(List<Node> list, float enemyRatio, int maxEnemyCount)
  {
    var restEnemyCount = Mathf.FloorToInt(graph.NodeCount * enemyRatio);
    for(var i = list.Count - 1; i >= 0; i--)
    {
      var n = list[i];
      list.RemoveAt(i);
      var enemyCount = Mathf.Min(restEnemyCount, Random.Range(1, maxEnemyCount));
      n.EnemyCount.Value = enemyCount;
      restEnemyCount -= 1;
      //enemyCount;
      if(restEnemyCount <= 0)
      {
        break;
      }
    }
    return list;
  }

  void createExit()
  {
    var exitNode = graph.NodeList[Random.Range(0, graph.NodeCount)];
    addNodeView(exitNode);
    exitNode.isExit.Value = true;
    exitNode.IsVisited.Value = true;
  }

  void addItems()
  {
    var deadends = graph
      .DeadendNodeList
      .Where(n => Random.value < itemProb);

    foreach(var n in deadends)
    {
      n.HasItem.Value = true;
    }
  }

  void setPlayerPos(List<Node> list)
  {
    Node n;
    for(var i = list.Count - 1; i >= 0; i--)
    {
      n = list[i];
      list.RemoveAt(i);
      if(graph.ScanEnemies(n.Coords) > 0)
      {
        continue;
      }
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
    var divPt = Random.Range(margin, longSide - 1 - margin);

    //connect divided rects
    connectArea(
      (IntVector2)rect.min + (isVerticalDivide ? new IntVector2(divPt, 0) : new IntVector2(0, divPt)),
      shortSide,
      isVerticalDivide
    );

    //divide more if has enough area
    if(area > 2)
    {
      divRect1 = divRect2 = rect;

      divPt++;
      if(isVerticalDivide)
      {
        divRect1.width = divPt;
        divRect2.xMin += divPt;
      }
      else
      {
        divRect1.height = divPt;
        divRect2.yMin += divPt;
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

    var connectCount = (float)lineLength * connectPerLength;

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

  void addNodeView(Node node)
  {
    if(node.HasView)
      return;

    var go = Instantiate(gridNode3DPrefab, CoordsToVec3(node.Coords), Quaternion.identity) as GameObject;
    go.transform.SetParent(viewContainer, false);
    go.GetComponent<Node3DPresenter>().Model = node;
    go.name = "node_" + coordsToObjectName(node.Coords);
    node.HasView = true;
  }
  void addEdgeView(Edge edge)
  {
    if(edge.HasView)
      return;
    var go = Instantiate(gridEdge3DPrefab, CoordsToVec3(edge.Coords), Quaternion.Euler(new Vector3(0, edge.Deg, 0))) as GameObject;
    go.transform.SetParent(viewContainer, false);
    go.GetComponent<EdgePresenter>().Model = edge;
    go.name = "edge_" + coordsToObjectName(edge.SourceNode.Coords) + "-" + coordsToObjectName(edge.TargetNode.Coords);
    edge.HasView = true;
  }

  public Vector3 CoordsToVec3(Vector2 coords)
  {
    return new Vector3(coords.x * gridUnit, 0, coords.y * gridUnit);
  }
  string coordsToObjectName(IntVector2 coords)
  {
    return coords.X + "," + coords.Y;
  }

}
