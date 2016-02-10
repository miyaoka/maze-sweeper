using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;

public class SectorListManager
{  
  public List<List<Sector>> SectorList = new List<List<Sector>>();
  //col
  int sectorCount = 7;
  //row
  int floorCount = 6;
  int maxFloorSize = 3;
  public ReactiveProperty<int> Update = new ReactiveProperty<int>();
  public ReactiveProperty<Sector> CurrentSector = new ReactiveProperty<Sector>();

  List<SectorType> SectorTypeList = new List<SectorType>()
  {
    new SectorType("Engine", new Color(.8f, .4f, .4f), .12f),
    new SectorType("Armory", new Color(.8f, .4f, .8f), .11f),
    new SectorType("Lab", new Color(.5f, .7f, .9f), .1f),
    new SectorType("System", new Color(.8f, .8f, 0), .1f),
    new SectorType("Living", new Color(.5f, .9f, .5f), .09f),
    new SectorType("Cargo", new Color(.7f, .7f, .7f), .08f)
  };

  private static SectorListManager instance;
  public static SectorListManager Instance
  {
    get
    {
      if (instance == null)
      {
        instance = new SectorListManager();
      }
      return instance;
    }
  }

  private SectorListManager() {

  }

  public void Init()
  {
    SectorList.Clear();
    CurrentSector.Value = new Sector(floorCount, 0, 0);

    for (var i = 0; i < sectorCount; i++)
    {
      SectorList.Add(sectorList(i));
    }
    SectorList.Add(new List<Sector>() {
      new Sector(floorCount, sectorCount, 0,
      new SectorType("Shuttle", Color.black, .13f))
    });

    CurrentSector.Value = SectorList[0][0];

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

  float colSizeBase = 12;
  float colSizePerLevel = 2;
  float rowSizeBase = 15;
  float rowSizePerLevel = 5;
  float maxEnemyCountBase = 1.5f;
  float maxEnemyCountPerLevel = .5f;
  float rowSizePerFloor = .25f;

  public LevelConfig Conf(Sector sector)
  {
//    Debug.Log(sector.Type);
    var conf = new LevelConfig(
      Mathf.FloorToInt(sector.Level * colSizePerLevel + colSizeBase),
      Mathf.FloorToInt((sector.Level * rowSizePerLevel + rowSizeBase) * ((sector.FloorSize - 1) * rowSizePerFloor + 1)),
      sector.Type.EnemyRate,
      Mathf.FloorToInt(sector.Level * maxEnemyCountPerLevel + maxEnemyCountBase)
      );

//    Debug.Log(string.Format("{0},{1},{2}", conf.Col, conf.Row, conf.MaxEnemyCount));

    return conf;
  }
}
