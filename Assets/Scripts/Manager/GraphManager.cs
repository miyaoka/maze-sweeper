using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System.Collections;
using UniRx;

public enum Dirs { East, North, West, South, Null };
public class GraphManager : SingletonMonoBehaviour<GraphManager>
{
  [SerializeField]
  Transform viewContainer;
  [SerializeField]
  public GameObject exitZone;
  [SerializeField]
  GameObject gridNodePrefab;
  [SerializeField]
  GameObject gridEdgePrefab;
  [SerializeField]
  GameObject gridWallPrefab;
  [SerializeField]
  float GridUnit = 10;
  [SerializeField]
  float deadEndReduceProb = .5f;

  float itemsPerNode = .015f;
  [SerializeField]
  bool showAll;
  [SerializeField]
  GameObject explosionPrefab;

  public readonly GraphModel graph = new GraphModel();
  Dictionary<string, Wall> wallDict = new Dictionary<string, Wall>();

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
  public Node InitGrid(RoundConfig roundConf)
  {
    wallDict.Clear();
    clearView();
    graph.Clear();
    graph.CreateMaze(new Rect(0, 0, roundConf.Col, roundConf.Row));

    var count = 2;
    while (count-- > 0)
    {
      graph.RemoveDeadend(deadEndReduceProb);
    }
    graph.UpdateMaxCoords();

    //enemy
    var list = graph
      .ShuffledNodeList
      .Where(n => n.Coords.Y > 1)
      .ToList<Node>();
    addEnemies(list, roundConf.EnemyRate, roundConf.MaxEnemyCount);

    //exit zone
    exitZone.transform.localPosition = new Vector3(
      graph.MaxCoords.X * .5f * GridUnit,
      exitZone.transform.localPosition.y, 
      (graph.MaxCoords.Y -.5f) * GridUnit);

    //items
    addItems();

    //fire
    addFire();

//    addResucuee();

    return pickupPlayerPos(graph.NodeList.Where(n => n.Coords.Y == 0).ToList<Node>());
  }

  public void ShowAllNode()
  {
    var t = System.DateTime.Now;
    foreach (var n in graph.NodeList)
    {
      addNodeView(n);
      n.AlertCount.Value = graph.ScanEnemies(n.Coords);
      n.IsScanned.Value = true;
      addEdgeViews(n);
    }
    var t2 = System.DateTime.Now;
    Debug.Log("Instantiate: " + (t2 - t));
  }


  /// <summary>
  /// add node view with edge
  /// </summary>
  /// <param name="coord"></param>
  /// <param name="visit"></param>
  /// <returns></returns>
  public Node ShowNode(IntVector2 coord, bool visit = true)
  {
    var n = graph.GetNode(coord);
    if (n == null)
    {
      return null;
    }
    n.IsScanned.Value = visit || n.IsScanned.Value;

    addNodeView(n);
    addEdgeViews(n);
    return n;
  }
  void addEdgeViews(Node node)
  {
    node.EdgeArray
      .Select((v, i) => new { Value = v, Index = i })
      .ToList()
      .ForEach(e => {
        if(e.Value == null)
        {
          addWallView(node.Coords, e.Index);
        }
        else
        {
          addEdgeView(e.Value, e.Index);
        }
      });
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
    RoundManager.Instance.AlertCount.Value = node.AlertCount.Value = graph.ScanEnemies(node.Coords);
  }

  void addEnemies(List<Node> list, float enemyRatio, int maxEnemyCount)
  {
    var restEnemyCount = Mathf.FloorToInt(graph.NodeCount * enemyRatio);
    var i = 0;
    foreach (var n in list)
    {
      i++;
      var enemyCount = Mathf.Min(restEnemyCount, Random.Range(1, maxEnemyCount + 1));
      n.EnemyCount.Value = enemyCount;
      restEnemyCount -= 1;
      //enemyCount;
      if (restEnemyCount <= 0)
      {
        break;
      }
    }
    //add strong enemy
    list[0].EnemyCount.Value = 10;

    list.RemoveRange(0, i);
  }

  void createExit(List<Node> list)
  {
    var node = list[Random.Range(0, list.Count)];
    addNodeView(node);
    node.isExit.Value = true;
//    exitNode.IsVisited.Value = true;
  }

  void addItems()
  {
    var nodes = graph
      .NodeList
      .Where(n => n.Coords.Y > 0 && n.Coords.Y < graph.MaxCoords.Y)
      .ToList();

    nodes.Sort((a, b) => Random.value < .5f ? -1 : 1);

    var itemCount = nodes.Count() * itemsPerNode;
    Debug.Log(nodes.Count());
    Debug.Log(itemCount);

    foreach (var n in nodes)
    {
      if (itemCount-- <= 0)
      {
        return;
      }
      n.HasItem.Value = true;
      addNodeView(n);
//      n.IsVisited.Value = true;
    }
  }

  void addFire()
  {
    var nodes = graph.ShuffledNodeList;

    nodes.ForEach(n =>
    {
      n.HasFire.Value = Random.value < .3f;
    });

  }

  void addResucuee()
  {
    var nodes = graph.ShuffledNodeList;

    nodes.ForEach(n =>
    {
      n.HasRescuee.Value = Random.value < 1f;
    });
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
    if (node.HasView.Value)
      return;

    var go = Instantiate(gridNodePrefab, CoordsToVec3(node.Coords), Quaternion.identity) as GameObject;
    AddToView(go);
    go.GetComponent<NodePresenter>().Node = node;
    go.name = "node_" + coordsToObjectName(node.Coords);
  }
  void addEdgeView(Edge edge, int dir, bool explode = false)
  {
    if (edge.HasView)
      return;
    var go = Instantiate(gridEdgePrefab, CoordsToVec3(edge.Coords), Quaternion.Euler(new Vector3(0, dir * -90, 0))) as GameObject;
    AddToView(go);
    var ep = go.GetComponent<EdgePresenter>();
    ep.Edge = edge;
    if (explode)
    {
      ep.breach();
    }
    go.name = "edge_" + coordsToObjectName(edge.SourceNode.Coords) + "-" + coordsToObjectName(edge.TargetNode.Coords);
    edge.HasView = true;
  }
  void addWallView(IntVector2 coords, int dir)
  {
    var wall = new Wall(coords, dir);
    var key = wall.Key;
    if (wallDict.ContainsKey(key))
    {
      return;
    }
    wallDict[key] = wall;
    var go = Instantiate(gridWallPrefab, CoordsToVec3(wall.Coords), Quaternion.Euler(new Vector3(0, dir * -90, 0))) as GameObject;
    go.GetComponent<WallPresenter>().Wall = wall;
    go.name = "wall_" + key;
    AddToView(go);
  }
  public void BreachWall(WallPresenter wallView)
  {
    var w = wallView.Wall;
    var e = graph.CreateEdge(w.SourceCoords, w.TargetCoords);
    addEdgeView(e, w.Dir, true);

    Destroy(wallView.gameObject);
    wallDict.Remove(w.Key);
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
  public bool isExit(Node node)
  {
    return node.Coords.Y >= graph.MaxCoords.Y;
  }
}
