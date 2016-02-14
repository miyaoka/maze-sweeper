using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Dir { East, North, West, South };
public struct DirVector {

  static Dictionary<Dir, IntVector2> dict = new Dictionary<Dir, IntVector2>()
  {
    { Dir.East, new IntVector2(1, 0) },
    { Dir.North, new IntVector2(0, 1) },
    { Dir.West, new IntVector2(-1, 0) },
    { Dir.South, new IntVector2(0, -1) }
  };
  public static Dir VecToDir (IntVector2 vec) {
    return dict.First(v => v.Value == vec).Key;
	}
  public static Vector2 DirToVec(Dir dir)
  {
    return dict[dir];
  }
}
