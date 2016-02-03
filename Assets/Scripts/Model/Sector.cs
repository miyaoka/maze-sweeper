using UnityEngine;
using System.Collections;
using UniRx;

public class SectorType
{
  public string Name;
  public Color Color;
  public float EnemyRate;

  public SectorType(string type, Color color, float enemyRate)
  {
    Name = type;
    Color = color;
    EnemyRate = enemyRate;
  }
}

public enum SectorState { Current, Next, Visited, None };

public class Sector
{
  public SectorType Type;
  public int FloorSize;
  public int FloorIndex;
  public int Level;
  public bool hasSurvivor;
  public bool IsVisited;
  //  public ReactiveProperty<bool> IsVisited = new ReactiveProperty<bool>();

  public Sector(int floorSize, int level, int floorIndex, SectorType type = null)
  {
    Type = type;
    FloorSize = floorSize;
    Level = level;
    FloorIndex = floorIndex;

  }

  public int Bottom
  {
    get
    {
      return FloorIndex + FloorSize;
    }
  }
  public bool IsNextFrom(Sector fromSector)
  {
    return fromSector.Level + 1 == Level
        && fromSector.FloorIndex <= Bottom
        && fromSector.Bottom >= FloorIndex;
  }

  public SectorState State(Sector currentSector){
    return currentSector == null ? SectorState.None
        : IsNextFrom(currentSector) ? SectorState.Next
        : this == currentSector ? SectorState.Current
        : IsVisited ? SectorState.Visited
        : SectorState.None;
  }

}
