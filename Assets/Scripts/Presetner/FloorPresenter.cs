using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FloorPresenter : MonoBehaviour
{
  [SerializeField]
  RawImage blackLT;
  [SerializeField]
  RawImage blackRT;
  [SerializeField]
  RawImage blackLB;
  [SerializeField]
  RawImage blackRB;
  [SerializeField]
  RawImage whiteLT;
  [SerializeField]
  RawImage whiteRT;
  [SerializeField]
  RawImage whiteLB;
  [SerializeField]
  RawImage whiteRB;

  const int DirNorth = 1 << 0;
  const int DirEast = 1 << 1;
  const int DirSouth = 1 << 2;
  const int DirWest = 1 << 3;
  const int DirAll = DirNorth | DirEast | DirSouth | DirWest;

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
    if((lb & DirEast) != 0)
    {
      reqDir |= DirWest;
    }
    else
    {
      availableDir ^= DirWest;
    }
    var rb = floorTile(reqDir, availableDir);

    reqDir = 0;
    availableDir = DirAll;
    if((lb & DirNorth) != 0)
    {
      reqDir |= DirSouth;
    }
    else
    {
      availableDir ^= DirSouth;
    }
    var lt = floorTile(reqDir, availableDir);

    reqDir = 0;
    availableDir = DirAll;
    if((rb & DirNorth) != 0)
    {
      reqDir |= DirSouth;
    }
    else
    {
      availableDir ^= DirSouth;
    }
    if((lt & DirEast) != 0)
    {
      reqDir |= DirWest;
    }
    else
    {
      availableDir ^= DirWest;
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
    blackLT.uvRect = whiteLT.uvRect = uvRect(lt, xDiff1, yDiff1);
    blackRT.uvRect = whiteRT.uvRect = uvRect(rt, xDiff2, yDiff1);
    blackLB.uvRect = whiteLB.uvRect = uvRect(lb, xDiff1, yDiff2);
    blackRB.uvRect = whiteRB.uvRect = uvRect(rb, xDiff2, yDiff2);
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
  Rect uvRect(int index, float xDiff, float yDiff)
  {
    var sliceUnit = 1f / 4f;
    var diffUnit = 1.2f / 16f;

    xDiff = (xDiff - .5f) * diffUnit;
    yDiff = (yDiff - .5f) * diffUnit;
    var x = index % 4 + .25f;
    var y = 3 - Mathf.Floor((float)index / 4f) + .25f;

    return new Rect(
      x * sliceUnit + xDiff, y * sliceUnit + yDiff,
      .125f, .125f
    );
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
