using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;
using System.Linq;

public class Node
{
  public readonly IntVector2 Coords;
  public ReactiveProperty<int> Degree = new ReactiveProperty<int>();
  public ReactiveProperty<int> EnemyCount = new ReactiveProperty<int>(0);
  public ReactiveProperty<int> AlertCount = new ReactiveProperty<int>(-1);
  public ReactiveProperty<bool> HasView = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasGuideView = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsScanned = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> IsVisited = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnHere = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> OnDest = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> isExit = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasEnergy = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasRescuee = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasItem = new ReactiveProperty<bool>();
  public ReactiveProperty<bool> HasFire = new ReactiveProperty<bool>();


  public readonly Edge[] EdgeArray = new Edge[4];

  public ReactiveProperty<int> DangerLevel = new ReactiveProperty<int>();
  CompositeDisposable neighborResources = new CompositeDisposable();

  public List<Edge> EdgeList
  {
    get
    {
      return EdgeArray
        .Where(e => e != null)
        .ToList<Edge>();
    }
  }

  public Node(IntVector2 coords)
  {
    this.Coords = coords;

    OnHere =
      PlayerManager.Instance
        .CurrentCoords
        .Select(n => n == coords)
        .DistinctUntilChanged()
        .ToReactiveProperty();
    OnDest =
      PlayerManager.Instance
        .DestCoords
        .Select(n => n == coords)
        .DistinctUntilChanged()
        .ToReactiveProperty();

  }

  public void SetNeighborList(List<Node> nNodeList)
  {
    neighborResources.Clear();
    Observable
      .CombineLatest(
        nNodeList
        .Select(n => n.AlertCount)
        .ToArray()
      )
     .Subscribe(alertList =>
      {
        /*
        var str = "";
        alertList
          .ToList()
          .ForEach(ac => {
            if (ac > -1)
            {
              str += ac.ToString() + ",";
              }
          });

        Debug.Log(Coords + ": " + (str == "" ? "---" : str));
        */

        var al =
        alertList
          .Where(ac => ac > -1)
          .ToList();

        DangerLevel.Value =
        (al.Count == 0 || al.Min() == 0) 
        ? 0
        : al.Where(ac => ac > 0).Count();
      })
      .AddTo(neighborResources);
  }
  /*
  public void UpdateNeighborInfo(Node nn)
  {
    var localCoords = nn.Coords - Coords;
    if(neighborEnemyDict.ContainsKey(localCoords))
    {
      neighborEnemyDict[localCoords] = nn.IsSafe.Value;
    }
    else
    {
      neighborEnemyDict.Add(localCoords, nn.IsSafe.Value);
    }


    var safeZoneDict = new Dictionary<IntVector2, bool>();

    neighborEnemyDict
      .Keys
      .ToList()
      .ForEach(c =>
      {
        var isSafe = neighborEnemyDict[c];
        if (!isSafe)
        {
          return;
        }
        for (var x = -1; x <= 1; x++)
        {
          for (var y = -1; y <= 1; y++)
          {
            var c2 = c + new IntVector2(x, y);
            if (!safeZoneDict.ContainsKey(c2))
            {
              safeZoneDict.Add(c2, true);
            }
          }
        }
      });

    DangerZoneList.Clear();

    for (var x = -1; x <= 1; x++)
    {
      for (var y = -1; y <= 1; y++)
      {
        var c = new IntVector2(x, y);
        if(!safeZoneDict.ContainsKey(c))
        {
          Debug.Log("d add:" + c);
          DangerZoneList.Add(c);
        }
        
      }
    }
  }
  */

  public void AddEdge(Edge e)
  {
    int d = edgeAngleIndex(e);
    EdgeArray[d] = e;

    //    EdgeList.Add(e);
    updateDegree();
  }

  public void RemoveEdge(Edge e)
  {
    int d = edgeAngleIndex(e);
    EdgeArray[d] = null;
    //    EdgeList.Remove(e);
    updateDegree();
  }

  private int edgeAngleIndex(Edge e)
  {
    //normalize degree to 0 - 3
    return ((int)e.GetAngleFromNode(this) + 360) % 360 / 90;
  }

  void updateDegree()
  {
    Degree.Value = EdgeList.Count;
  }

  public event EventHandler OnDestroy;
  public void destroy()
  {
    if (OnDestroy != null)
    {
      OnDestroy(this, EventArgs.Empty);
    }
    Degree.Dispose();
  }


}
