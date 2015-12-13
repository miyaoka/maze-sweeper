using UnityEngine;
using System.Collections;
using UniRx;
using System.Collections.Generic;

public class EdgeModel {

  public List<NodeModel> nodes;
  public Dirs dir;
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