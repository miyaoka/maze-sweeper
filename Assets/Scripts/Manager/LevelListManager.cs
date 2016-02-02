using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;

public class LevelListManager : SingletonMonoBehaviour<LevelListManager>
{  
  public List<List<Sector>> LevelList = new List<List<Sector>>();
  int levelCount = 7;
  int floorCount = 6;
  int maxFloorSize = 3;
  public ReactiveProperty<int> Update = new ReactiveProperty<int>();
  public ReactiveProperty<Sector> CurrentSector = new ReactiveProperty<Sector>();

  //  public enum SectorType { Engine, Weapon, Cargo, Living, System, Lab };

  List<SectorType> SectorTypeList = new List<SectorType>()
  {
    new SectorType("Engine", new Color(.8f, .4f, .4f)),
    new SectorType("Cargo", new Color(.7f, .7f, .7f)),
    new SectorType("Lab", new Color(.5f, .7f, .9f)),
    new SectorType("Armory", new Color(.8f, .4f, .8f)),
    new SectorType("System", new Color(.8f, .8f, 0)),
    new SectorType("Living", new Color(.5f, .9f, .5f))
  };

  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }

  }

  void Start () {

    Init();
  }

  public void Init()
  {
    LevelList.Clear();
    CurrentSector.Value = new Sector(floorCount, -1, 0);

    for (var i = 0; i < levelCount; i++)
    {
      LevelList.Add(sectorList(i));
    }
    LevelList.Add(new List<Sector>() {
      new Sector(floorCount, levelCount, 0,
      new SectorType("Shuttle", Color.black))
    });

    Update.Value += 1;

  }
  List<Sector> sectorList(int level)
  {
    var sectorTypeList = SectorTypeList;
    sectorTypeList
      .Sort((a, b) => Random.value < .5f ? -1 : 1);


    var list = new List<Sector>();
    var restFloorSize = floorCount;
    var floorIndex = 0;
    for (var t = 0; t < sectorTypeList.Count; t++)
    {
      var sectorType = sectorTypeList[t];
      var floorSize = Mathf.Min(restFloorSize, Random.Range(1, maxFloorSize + 1));
      restFloorSize -= floorSize;
      var sector = new Sector(floorSize, level, floorIndex, sectorType);
      floorIndex += floorSize;

      sector.hasSurvivor = Random.value < .25f;

      list.Add(sector);
      if(restFloorSize <= 0)
      {
        break;
      }
    }
    return list;
  }
	
}
