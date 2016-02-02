using UnityEngine;
using System.Collections;

public class SectorListPresenter : MonoBehaviour
{

  Sector sector;
  void Start()
  {

  }

  public Sector Sector
  {
    set
    {
      this.sector = value;

    }
  }
}
