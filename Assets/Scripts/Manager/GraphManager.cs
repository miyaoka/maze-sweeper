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
  GameObject gridNodePrefab;
  [SerializeField]
  GameObject gridEdgePrefab;
  [SerializeField]
  float GridUnit = 10;
  [SerializeField]
  float deadEndReduceProb = .5f;
  [SerializeField]
  float itemProb = .3f;
  [SerializeField]
  bool showAll;

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
  public Node InitGrid(int gridWidth, int gridHeight, float enemyRatio)
  {
    graph.Clear();
    graph.CreateMaze(new Rect(0, 0, gridWidth, gridHeight));

    var count = 2;
    while(count-- > 0)
    {
      graph.RemoveDeadend(deadEndReduceProb);
    }
    var list = graph
      .ShuffledNodeList
      .Where(n => n.Coords.Y > 1)
      .ToList<Node>();
    Debug.Log(list.Count);
    addEnemies(list, enemyRatio, 2);
    Debug.Log(list.Count);

    count = 3;
    while(count-- > 0)
    {
      createExit();
    }
    addItems();

    if(showAll)
    {
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

    return pickupPlayerPos(graph.NodeList.Where(n => n.Coords.Y == 0).ToList<Node>());
  }

  public Node VisitNode(IntVector2 coord)
  {
    var n = graph.GetNode(coord);
    if(n == null)
    {
      return null;
    }
    if(!n.IsVisited.Value)
    {
      n.IsVisited.Value = true;
      addNodeView(n);

      foreach(var e in n.EdgeList)
      {
        addEdgeView(e);
      }
    }
    return n;
  }
  public void ClearNodeEnemy(Node node)
  {
    foreach(var n in graph.Neighbors(node.Coords))
    {
      n.AlertCount.Value = Mathf.Max(0, n.AlertCount.Value - node.EnemyCount.Value);
    }
    node.EnemyCount.Value = 0;
  }

  void addEnemies(List<Node> list, float enemyRatio, int maxEnemyCount)
  {
    var restEnemyCount = Mathf.FloorToInt(graph.NodeCount * enemyRatio);
    var i = 0;
    foreach(var n in list)
    {
      i++;
      var enemyCount = Mathf.Min(restEnemyCount, Random.Range(1, maxEnemyCount));
      n.EnemyCount.Value = enemyCount;
      restEnemyCount -= 1;
      //enemyCount;
      if(restEnemyCount <= 0)
      {
        break;
      }
    }
    list.RemoveRange(0, i);
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

  Node pickupPlayerPos(List<Node> list)
  {
    foreach(Node n in list)
    {
      if(graph.ScanEnemies(n.Coords) == 0)
      {
        return n;
      }
    }
    throw new UnityException("No deployable node.");
  }

  void addNodeView(Node node)
  {
    if(node.HasView)
      return;

    var go = Instantiate(gridNodePrefab, CoordsToVec3(node.Coords), Quaternion.identity) as GameObject;
    go.transform.SetParent(viewContainer, false);
    go.GetComponent<NodePresenter>().Model = node;
    go.name = "node_" + coordsToObjectName(node.Coords);
    node.HasView = true;
  }
  void addEdgeView(Edge edge)
  {
    if(edge.HasView)
      return;
    var go = Instantiate(gridEdgePrefab, CoordsToVec3(edge.Coords), Quaternion.Euler(new Vector3(0, edge.Deg, 0))) as GameObject;
    go.transform.SetParent(viewContainer, false);
    go.GetComponent<EdgePresenter>().Model = edge;
    go.name = "edge_" + coordsToObjectName(edge.SourceNode.Coords) + "-" + coordsToObjectName(edge.TargetNode.Coords);
    edge.HasView = true;
  }

  public Vector3 CoordsToVec3(Vector2 coords)
  {
    return new Vector3(coords.x * GridUnit, 0, coords.y * GridUnit);
  }
  string coordsToObjectName(IntVector2 coords)
  {
    return coords.X + "," + coords.Y;
  }

}
