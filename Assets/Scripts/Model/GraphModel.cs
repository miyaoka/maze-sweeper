using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class GraphModel {
  public GraphModel(){
  }
  public readonly Dictionary<IntVector2, Node> nodeList = new Dictionary<IntVector2, Node> ();

  public Node createNode(IntVector2 coords, bool ifNotExist = false){
    if(ifNotExist && nodeList.ContainsKey (coords)){
      return nodeList [coords];
    }
    var n = new Node(coords);
    nodeList.Add (coords, n);
    return n;
  }
  public Edge createEdge(IntVector2 coords0, IntVector2 coords1){
    var n0 = createNode (coords0, true);
    var n1 = createNode (coords1, true);
    var edge = new Edge (n0, n1);
    return edge;
  }
  public Node getNode(IntVector2 coords){
    return nodeList.ContainsKey (coords) ? nodeList [coords] : null;
  }
  public void removeNode(IntVector2 coords){
    var n = getNode (coords);
    var list = new List<Edge> (n.edgeList);
    foreach (var e in list) {
      n.removeEdge (e);
      e.oppositeNode (n).removeEdge (e);
    }
    nodeList.Remove (coords);
  }
  public List<Node> deadendNodeList{
    get{
      return new List<Node>(nodeList.Values).Where (n => n.degree.Value == 1).ToList();
    }
  }
  public List<Node> shuffledNodeList{
    get{
      var list = new List<Node> (nodeList.Values);
      list.Sort ((a, b) => Random.value < .5f ? -1 : 1);
      return list;
    }
  }
  public void clear(){
    var nodes = nodeList.Values.ToArray<Node>();
    foreach (var n in nodes) {
      n.destroy ();
      removeNode(n.coords);
    }
    nodeList.Clear ();
  }

  public int scanEnemies(IntVector2 coords){
    var count = 0;
    foreach (var n in neighbors(coords)) {
      count += n.enemyCount.Value;
    }
    return count;
  }
  public List<Node> neighbors(IntVector2 coords) {
    var list = new List<Node> ();
    for (var nx = coords.x - 1; nx <= coords.x + 1; nx++) {
      for (var ny = coords.y - 1; ny <= coords.y + 1; ny++) {
        var dest = new IntVector2 (nx, ny);
        if (dest == coords) {
          continue;
        }

        var neighbor = getNode (dest);
        if (neighbor != null) {
          list.Add (neighbor);
        }
      }
    }
    return list;
  }
}
public class EdgeType {

  public const int DIR_TOP = 0;
  public const int DIR_RIGHT = 1;
  public ReactiveProperty<bool> isPassable = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isBreachable = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isBreached = new ReactiveProperty<bool>();
  public ReactiveProperty<int> doorCount = new ReactiveProperty<int>();

  public EdgeType(bool passable, bool breachable, int doors, bool breached = false){
    isPassable.Value = passable;
    isBreachable.Value = breachable;
    doorCount.Value = doors;
    isBreached.Value = breached;
  }
  public static EdgeType unbreachableWall { get { return new EdgeType (false, false, 0); } }
  public static EdgeType wall{ get{ return new EdgeType (false, true, 0); } }
  public static EdgeType passage{ get { return new EdgeType (true, false, 0); } }
  public static EdgeType door(int doors){
    return new EdgeType (false, true, doors);
  }
  public EdgeType breach(){
    if (!isBreachable.Value) {
      return this;
    }
    doorCount.Value = 0;
    isPassable.Value = true;
    isBreached.Value = true;
    return this;
  }
}
