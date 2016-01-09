using UnityEngine;
using System.Collections;
using UniRx;

public class Wall
{
  public readonly IntVector2 SourceCoords;
  public readonly IntVector2 TargetCoords;
  public readonly Vector2 Coords;
  public readonly int Dir;
  public ReactiveProperty<bool> OnHere = new ReactiveProperty<bool>();
  public Wall(IntVector2 coords, int dir)
  {
    SourceCoords = coords;
    TargetCoords = coords + GraphModel.DirCoords[dir];

    if (dir > 1)
    {
      SourceCoords = TargetCoords;
      TargetCoords = coords;
    }
    Coords = (Vector2)(SourceCoords + TargetCoords) * .5f;
    Dir = dir;

    OnHere = PlayerManager.Instance
    .CurrentCoords
    .Select(c => c == SourceCoords || c == TargetCoords)
    .ToReactiveProperty();
  }
  public string Key
  {
    get
    {
      return SourceCoords.X + "," + SourceCoords.Y + "_" + TargetCoords.X + "," + TargetCoords.Y;
    }
  }
}
