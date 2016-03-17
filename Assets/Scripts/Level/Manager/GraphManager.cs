using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System.Collections;
using UniRx;

public class GraphManager : SingletonMonoBehaviour<GraphManager>
{
  [SerializeField]
  Transform viewContainer;
  [SerializeField]
  public GameObject exitZone;
  [SerializeField]
  public GameObject dangerZone;
  [SerializeField]
  GameObject gridNodePrefab;
  [SerializeField]
  GameObject gridEdgePrefab;
  [SerializeField]
  GameObject gridWallPrefab;
  [SerializeField]
  GameObject guideNodePrefab;
  [SerializeField]
  float GridUnit = 10;
  [SerializeField]
  float deadEndReduceProb = .5f;

  float energyPerNode = .015f;
  float itemsPerNode = .01f;
  [SerializeField]
  bool showAll;
  [SerializeField]
  GameObject explosionPrefab;

  public readonly Graph graph = new Graph();
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
  public Node InitGrid(LevelConfig roundConf)
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

    combineRoom();

    //exit zone
    exitZone.transform.localPosition = new Vector3(
      graph.MaxCoords.X * .5f * GridUnit,
      exitZone.transform.localPosition.y, 
      (graph.MaxCoords.Y -.5f) * GridUnit);

    dangerZone.transform.localPosition = new Vector3(
      graph.MaxCoords.X * .5f * GridUnit,
      dangerZone.transform.localPosition.y,
      (0 - .5f) * GridUnit);

    //items
//    addItems();

    //fire
//    addFire();

//    addResucuee();

    return pickupPlayerPos();
  }

  public void ShowAllNode()
  {
    var t = System.DateTime.Now;
    graph.NodeList
      .ForEach(addNodeAndEdgeView);

    var t2 = System.DateTime.Now;
    Debug.Log("Instantiate: " + (t2 - t));
  }
  void addNodeAndEdgeView(Node n)
  {
    n.IsScanned.Value = true;
    addNodeView(n);
    addEdgeViews(n);
  }

  void combineRoom()
  {
    var nodeList = graph.NodeList;

    //prescan all node
    nodeList
      .ForEach(n =>
      {
        n.AlertCount.Value = graph.ScanEnemies(n.Coords);
      });

    //each no alert node
    nodeList
      .Where(n => n.AlertCount.Value == 0)
      .ToList()
      .ForEach(noAlertNode =>
      {
        var neighborNodes = noAlertNode.ReachableNeighborNodeList();

        //has enemy in 3x3
        if (neighborNodes.Count != 9)
        {
          return;
        }

        //combine nodes
        noAlertNode.IsRoomCenter = true;
        neighborNodes.ForEach(n =>
        {
          n.IsRoom = true;
          Graph.NextGridCoords
          .Select((v, i) => new { Value = v, Index = i })
          .ToList()
          .ForEach(nc =>
          {
            var nextCoords = n.Coords + nc.Value;
            if (!neighborNodes.Contains(graph.GetNode(nextCoords)))
            {
              return;
            }
            var e = n.EdgeArray[nc.Index];
            if (e == null)
            {
              e = graph.CreateEdge(n.Coords, nextCoords);
            }
            e.noWall.Value = true;
          });
        });
      });

    //set rooms
    var roomList = new List<IntVector2>();
    nodeList
      .Where(n => n.IsRoom)
      .ToList()
      .ForEach(roomNode =>
      {
        if(roomNode.RoomRootCoords.HasValue)
        {
          return;
        }
        setRoom(roomNode, roomNode.Coords);
        roomList.Add(roomNode.Coords);
      });


    var maxItemPerRoom = 2;
    roomList
      .ForEach(r =>
      {
        var roomCenterNodeList =
        nodeList
        .Where(n => n.RoomRootCoords == r)
        .Where(n => n.IsRoomCenter)
        .ToList();

        var c = Random.Range(1, Mathf.Min(roomCenterNodeList.Count, maxItemPerRoom) + 1);

        roomCenterNodeList.Sort((a, b) => Random.value < .5f ? -1 : 1);

        roomCenterNodeList
        .ForEach(n =>
        {
          if (c-- <= 0)
          {
            return;
          }
          n.HasItem.Value = true;
        });

      });

    //create view
    nodeList
      .Where(n => n.IsRoom)
      .ToList()
      .ForEach(n => {
        //        n.IsVisited.Value = true;
        n.AlertCount.Value = -1;
        addNodeAndEdgeView(n);
        n.IsScanned.Value = false;
      });
  }

  void setRoom(Node n, IntVector2 rootCoords)
  {
    if(n.RoomRootCoords.HasValue || !n.IsRoom)
    {
      return;
    }
    n.RoomRootCoords = rootCoords;

    n.EdgeList
    .Where(e => e.noWall.Value)
    .Select(e => e.OppositeNode(n))
    .ToList()
    .ForEach(nn =>
    {
      setRoom(nn, rootCoords);
    });
  }

  /// <summary>
  /// add node view and visit
  /// </summary>
  /// <param name="coords"></param>
  /// <returns></returns>
  public Node ShowNode(IntVector2 coords)
  {
    var n = showNode(coords, new List<Node>());
    if (n != null)
    {
      LevelManager.Instance.AlertCount.Value = n.AlertCount.Value;
    }
    return n;
  }
  Node showNode(IntVector2 coords, List<Node> seeked)
  {
    var n = graph.GetNode(coords);
    if (n == null)
    {
      return null;
    }
    if(n.IsVisited.Value)
    {
      return n;
    }
    n.IsVisited.Value = true;
    n.AlertCount.Value = graph.ScanEnemies(n.Coords);

    if (seeked.Contains(n))
    {
      return n;
    }
    seeked.Add(n);

    addNodeAndEdgeView(n);


    n.EdgeList
      .Where(e => e.noWall.Value)
      .Select(e => e.OppositeNode(n).Coords)
      .ToList()
      .ForEach(c =>
      {
        var nn = showNode(c, seeked);
      });

    return n;
  }

  public void ClearNodeEnemy(Node node)
  {
    graph.Neighbors(node.Coords, true)
      .Where(n => n.IsScanned.Value)
      .ToList()
      .ForEach(n =>
      {
        n.AlertCount.Value = Mathf.Max(0, n.AlertCount.Value - node.EnemyCount.Value);
      });

    node.EnemyCount.Value = 0;
//    ScanEnemies(node);
  }
  public void BreachNode(IntVector2 coords)
  {
    var n = showNode(coords, new List<Node>());
    ClearNodeEnemy(n);
//    addNodeAndEdgeView(n);
    AudioManager.Instance.Play(AudioName.Breach);
    var explosionObj = Instantiate(explosionPrefab, CoordsToVec3(coords), Quaternion.identity) as GameObject;
    AddToView(explosionObj);
    Destroy(explosionObj, 3f);
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

    var energyCount = nodes.Count() * energyPerNode;
    var itemCount = nodes.Count() * itemsPerNode;
    var rescueeCount = Random.Range(1, 3);

    var i = 0;

    for (; i < nodes.Count; i++)
    {
      var n = nodes[i];
      if (rescueeCount-- <= 0)
      {
        break;
      }
      n.HasRescuee.Value = true;
      addNodeView(n);
    }
    for (; i < nodes.Count; i++)
    {
      var n = nodes[i];
      if (energyCount-- <= 0)
      {
        break;
      }
      n.HasEnergy.Value = true;
      addNodeView(n);
    }
    for (; i < nodes.Count; i++)
    {
      var n = nodes[i];
      if (itemCount-- <= 0)
      {
        break;
      }
      n.HasItem.Value = true;
      addNodeView(n);
    }

  }

  void addFire()
  {
    var nodes = graph.ShuffledNodeList;

    nodes.ForEach(n =>
    {
      n.HasFire.Value = Random.value < .05f;
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

  Node pickupPlayerPos()
  {
    var list = graph
      .NodeList
      .Where(n => n.Coords.Y == 0)
      .Where(n => graph.ScanEnemies(n.Coords) == 0)
      .ToList<Node>();
    if(list.Count == 0)
    {
      throw new UnityException("No deployable node.");
    }

    return list[Random.Range(0, list.Count)];
  }

  void addNodeView(Node node)
  {
    if (node.HasView.Value)
      return;

    node.HasView.Value = true;

    var go = Instantiate(gridNodePrefab, CoordsToVec3(node.Coords), Quaternion.identity) as GameObject;
    AddToView(go);
    go.GetComponent<NodePresenter>().Node = node;
    go.name = "node_" + coordsToObjectName(node.Coords);

    //add guide
    getOrCreateNeighborNodeList(node)
    .ForEach(addGuideNodeView);
  }
  void addGuideNodeView(Node node)
  {
    if (node.HasGuideView.Value)
      return;

    node.HasGuideView.Value = true;
    node.SetNeighborList(getOrCreateNeighborNodeList(node));

    var go = Instantiate(guideNodePrefab, CoordsToVec3(node.Coords), Quaternion.identity) as GameObject;
    AddToView(go);
    go.GetComponent<GuideNodePresenter>().Node = node;
    go.name = "guide_node_" + coordsToObjectName(node.Coords);
  }
  List<Node> getOrCreateNeighborNodeList(Node node)
  {
    return
      graph.NeighborCoords(node.Coords)
      .Select(nc => graph.GetOrCreateNode(nc))
      .ToList();
  }

  void addEdgeViews(Node node)
  {
    node.EdgeArray
      .Select((v, i) => new { Value = v, Index = i })
      .ToList()
      .ForEach(e => {
        if (e.Value == null)
        {
          addWallView(node.Coords, e.Index);
        }
        else
        {
          addEdgeView(e.Value, e.Index);
        }
      });
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
