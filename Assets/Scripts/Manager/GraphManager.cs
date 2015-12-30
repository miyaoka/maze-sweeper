using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System.Collections;

public enum Dirs { East, North, West, South, Null };
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
  float itemsPerRow = .2f;
  [SerializeField]
  bool showAll;

  public readonly GraphModel graph = new GraphModel();

  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }
    foreach (Transform t in viewContainer)
    {
      Destroy(t.gameObject);
    }
  }
  void Start()
  {

  }
  public Node InitGrid(int gridWidth, int gridHeight, float enemyRatio)
  {
    clearView();
    graph.Clear();
    graph.CreateMaze(new Rect(0, 0, gridWidth, gridHeight));

    var count = 2;
    while (count-- > 0)
    {
      graph.RemoveDeadend(deadEndReduceProb);
    }

    //enemy
    var list = graph
      .ShuffledNodeList
      .Where(n => n.Coords.Y > 1)
      .ToList<Node>();
    Debug.Log(list.Count);
    addEnemies(list, enemyRatio, 3);
    Debug.Log(list.Count);

    //exit
    createExit(
      graph
      .NodeList
      .Where(n => (float)n.Coords.Y >= (float)gridHeight * .75f)
      .ToList()
      );
    createExit(
      graph
      .NodeList
      .Where(n => (float)n.Coords.Y < (float)gridHeight * .75f && (float)n.Coords.Y >= (float)gridHeight * .5f)
      .ToList()
      );

    //survivor
    addItems(Mathf.CeilToInt(gridHeight * itemsPerRow));

    return pickupPlayerPos(graph.NodeList.Where(n => n.Coords.Y == 0).ToList<Node>());
  }

  public void ShowAllNode()
  {
    var t = System.DateTime.Now;
    foreach (var n in graph.NodeList)
    {
      addNodeView(n);
      n.AlertCount.Value = graph.ScanEnemies(n.Coords);
      foreach (var e in n.EdgeList)
      {
        addEdgeView(e);
      }
    }
    var t2 = System.DateTime.Now;
    Debug.Log("Instantiate: " + (t2 - t));
  }

  public Node VisitNode(IntVector2 coord)
  {
    var n = graph.GetNode(coord);
    if (n == null)
    {
      return null;
    }
    if (!n.IsVisited.Value)
    {
      n.IsVisited.Value = true;
      addNodeView(n);

      foreach (var e in n.EdgeList)
      {
        addEdgeView(e);
      }
    }
    return n;
  }
  public void ClearNodeEnemy(Node node)
  {
    foreach (var n in graph.Neighbors(node.Coords))
    {
      n.AlertCount.Value = Mathf.Max(0, n.AlertCount.Value - node.EnemyCount.Value);
    }
    node.EnemyCount.Value = 0;
    ScanEnemies(node);
  }

  public void ScanEnemies(Node node)
  {
    GameManager.Instance.AlertCount.Value = node.AlertCount.Value = graph.ScanEnemies(node.Coords);
  }

  void addEnemies(List<Node> list, float enemyRatio, int maxEnemyCount)
  {
    var restEnemyCount = Mathf.FloorToInt(graph.NodeCount * enemyRatio);
    var i = 0;
    foreach (var n in list)
    {
      i++;
      var enemyCount = Mathf.Min(restEnemyCount, Random.Range(1, maxEnemyCount));
      n.EnemyCount.Value = enemyCount;
      restEnemyCount -= 1;
      //enemyCount;
      if (restEnemyCount <= 0)
      {
        break;
      }
    }
    list.RemoveRange(0, i);
  }

  void createExit(List<Node> list)
  {
    var node = list[Random.Range(0, list.Count)];
    addNodeView(node);
    node.isExit.Value = true;
//    exitNode.IsVisited.Value = true;
  }

  void addItems(int itemCount)
  {
    var deadends = graph.DeadendNodeList;
    deadends.Sort((a, b) => Random.value < .5f ? -1 : 1);

    foreach (var n in deadends)
    {
      if (itemCount-- == 0)
      {
        return;
      }
      n.HasItem.Value = true;
      addNodeView(n);
//      n.IsVisited.Value = true;
    }
  }

  Node pickupPlayerPos(List<Node> list)
  {
    foreach (Node n in list)
    {
      if (graph.ScanEnemies(n.Coords) == 0)
      {
        return n;
      }
    }
    throw new UnityException("No deployable node.");
  }

  void addNodeView(Node node)
  {
    if (node.HasView)
      return;

    var go = Instantiate(gridNodePrefab, CoordsToVec3(node.Coords), Quaternion.identity) as GameObject;
    AddToView(go);
    go.GetComponent<NodePresenter>().Node = node;
    go.name = "node_" + coordsToObjectName(node.Coords);
    node.HasView = true;
  }
  void addEdgeView(Edge edge)
  {
    if (edge.HasView)
      return;
    var go = Instantiate(gridEdgePrefab, CoordsToVec3(edge.Coords), Quaternion.Euler(new Vector3(0, edge.Angle, 0))) as GameObject;
    AddToView(go);
    go.GetComponent<EdgePresenter>().Edge = edge;
    go.name = "edge_" + coordsToObjectName(edge.SourceNode.Coords) + "-" + coordsToObjectName(edge.TargetNode.Coords);
    edge.HasView = true;
  }

  public void BreachWall(IntVector2 coords, int dir)
  {
    AudioManager.Breach.Play();
    var e = graph.CreateEdge(coords, coords + GraphModel.DirCoords[dir]);
    addEdgeView(e);
  }

  public Vector3 CoordsToVec3(Vector2 coords)
  {
    return new Vector3(coords.x * GridUnit, 0, coords.y * GridUnit);
  }
  string coordsToObjectName(IntVector2 coords)
  {
    return coords.X + "," + coords.Y;
  }
  public void AddToView(GameObject go)
  {
    go.transform.SetParent(viewContainer, false);
  }
  public void DestroyGrid()
  {
    foreach (Transform t in viewContainer)
    {
      StartCoroutine(addRigidBody(Random.Range(0, 2.0f), t.gameObject));
    }
  }
  IEnumerator addRigidBody(float waitTime, GameObject go)// UnityAction action)
  {
    yield return new WaitForSeconds(waitTime);
    go.AddComponent<Rigidbody>();
//    action();
  }
  void clearView()
  {
    foreach (Transform t in viewContainer)
    {
      Destroy(t.gameObject);
    }
  }
}
