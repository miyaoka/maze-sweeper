using UnityEngine;
using System.Collections;
using UniRx;
public class EnemyPresenter : MonoBehaviour {

  private MoverModel model;
  public MoverModel Model
  {
    set { 
      this.model = value; 



    }
    get { return this.model; }
  }
}
