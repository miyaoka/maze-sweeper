using UnityEngine;
using System.Collections;
using UniRx;
using System.Collections.Generic;

public class EdgeModel {

  public List<NodeModel> nodes;
  public Dirs dir;
  public bool visited = false;
  public ReactiveProperty<EdgeType> type = new ReactiveProperty<EdgeType> (EdgeType.passage);
  public EdgeModel(NodeModel node1, NodeModel node2, Dirs dir){
    nodes = new List<NodeModel>(){node1, node2};
    coordsList = new List<IntVector2> {node1.coords, node2.coords};
    this.dir = dir;
  }
  private List<IntVector2> coordsList;
  public List<IntVector2> CoordsList {
    get {
      return coordsList;
    }
  }
  public GameObject go;
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