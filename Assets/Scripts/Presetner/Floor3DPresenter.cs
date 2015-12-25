using UnityEngine;
using System.Collections;

public class Floor3DPresenter : MonoBehaviour
{
  [SerializeField]
  Renderer floorB_LT;
  [SerializeField]
  Renderer floorB_RT;
  [SerializeField]
  Renderer floorB_LB;
  [SerializeField]
  Renderer floorB_RB;

  const int DirT = 1 << 0;
  const int DirR = 1 << 1;
  const int DirB = 1 << 2;
  const int DirL = 1 << 3;
  const int DirAll = DirT | DirR | DirB | DirL;

  void Awake()
  {
    buildFloor();
  }

  void buildFloor()
  {
    var lb = floorTile(0, DirAll);
    int reqDir;
    int availableDir;

    reqDir = 0;
    availableDir = DirAll;
    if((lb & DirR) != 0)
    {
      reqDir |= DirL;
    }
    else
    {
      availableDir ^= DirL;
    }
    var rb = floorTile(reqDir, availableDir);

    reqDir = 0;
    availableDir = DirAll;
    if((lb & DirT) != 0)
    {
      reqDir |= DirB;
    }
    else
    {
      availableDir ^= DirB;
    }
    var lt = floorTile(reqDir, availableDir);

    reqDir = 0;
    availableDir = DirAll;
    if((rb & DirT) != 0)
    {
      reqDir |= DirB;
    }
    else
    {
      availableDir ^= DirB;
    }
    if((lt & DirR) != 0)
    {
      reqDir |= DirL;
    }
    else
    {
      availableDir ^= DirL;
    }
    var rt = floorTile(reqDir, availableDir);

    /*
    Debug.Log (binaryToString (lt) + "-" + binaryToString (rt));
    Debug.Log (binaryToString (lb) + "-" + binaryToString (rb));

    Debug.Log (binaryToString (reqDir));
    Debug.Log (binaryToString (availableDir));
*/
    var xDiff1 = Random.value;
    var yDiff1 = Random.value;
    var xDiff2 = Random.value;
    var yDiff2 = Random.value;
    floorB_LT.material.mainTextureOffset = offset(lt, xDiff1, yDiff1);
    floorB_RT.material.mainTextureOffset = offset(rt, xDiff2, yDiff1);
    floorB_LB.material.mainTextureOffset = offset(lb, xDiff1, yDiff2);
    floorB_RB.material.mainTextureOffset = offset(rb, xDiff2, yDiff2);
  }
  Vector2 offset(int index, float xDiff, float yDiff)
  {
    var sliceUnit = 1f / 4f;
    var diffUnit = 1.2f / 16f;

    xDiff = (xDiff - .5f) * diffUnit;
    yDiff = (yDiff - .5f) * diffUnit;

    var x = index % 4 + .25f;
    var y = 3 - Mathf.Floor((float)index / 4f) + .25f;
    return new Vector2(x * sliceUnit + xDiff, y * sliceUnit + yDiff);
  }

  int floorTile(int requireDir, int availableDir)
  {
    int tile = 0;
    while(tile == 0)
    {
      tile = (Random.Range(0, 1 << 4) | requireDir) & availableDir;
      //2辺に満たなければ空欄にしてやり直し
      if(bitCount(tile) < 2)
      {
        tile = 0;
      }
      //必須辺が無ければ空欄でも可にする
      if(requireDir == 0)
      {
        break;
      }
    }
    return tile;
  }
  int bitCount(int bit)
  {
    int count;
    for(count = 0; bit != 0; bit >>= 1)
    {
      if((bit & 1) != 0)
      {
        count++;
      }
    }
    return count;
  }
  string binaryToString(int i)
  {
    return System.Convert.ToString(i, 2).PadLeft(4, '0');
  }
}
