using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelConfig
{
  public int FloorNumber;
  public int Col;
  public int Row;
  public float EnemyRate;
  public int MaxEnemyCount;
  public List<Survivor> SurvivorList;
  public LevelConfig(int col, int row, float enemyRate, int maxEnemyCount)
  {
    Col = col;
    Row = row;
    EnemyRate = enemyRate;
    MaxEnemyCount = maxEnemyCount;
  }
}