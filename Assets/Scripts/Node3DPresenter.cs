using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using DG.Tweening;

public class Node3DPresenter : MonoBehaviour {
  [SerializeField] GameObject[] walls;
  [SerializeField] Light roomLight;
  [SerializeField] GameObject alert;
  [SerializeField] GameObject wallContainer;
  [SerializeField] GameObject tiles;

  CompositeDisposable modelResources = new CompositeDisposable();
  private NodeModel model;
  Tweener lightTw;
  TextMesh alertText;
  void Awake(){
    
    roomLight.intensity = 0;
    lightTw = roomLight.DOIntensity (0, 0);//.SetLoops (10, LoopType.Yoyo);
    roomLight.enabled = false;

    tiles.SetActive (false);
    alertText = alert.GetComponent<TextMesh> ();
  }
  public NodeModel Model
  {
    set { 
      this.model = value; 

      modelResources.Clear ();

      model.alertCount
        .DistinctUntilChanged()
        .Subscribe (c => {
          alertText.text = c == 0 ? "" : c.ToString ();
        })
        .AddTo (this);
      model.enemyCount
        .DistinctUntilChanged()
        .Subscribe (c => {
        })
        .AddTo (this);


      model.onHere
        .CombineLatest (model.onDest, (l, r) => l || r)
        .Subscribe (b => wallContainer.SetActive (b))
        .AddTo (this);
      model.onHere
        .Subscribe (b => {
          if(b) {
            //            sq.PrependInterval(.5f);
          }else{
            }
        })
        .AddTo (this);
      model.onDest
        .Subscribe (b => {
          if(b){
            tiles.SetActive(b);

            roomLight.enabled = true;
            lightTw.Kill();
            lightTw = roomLight.DOIntensity (1.2f, Random.Range(.8f, .8f)).SetEase (Ease.InQuad);
          }
          else{
            lightTw.Kill();
            lightTw = roomLight.DOIntensity (0.0f, Random.Range(.3f, .5f)).SetEase (Ease.OutQuad).SetDelay(.5f)
              .OnComplete(() =>{
                roomLight.enabled = false;
                tiles.SetActive(b);

              });
          }
        })
        .AddTo (this);
      model.visited
        .Where (b => b)
        .DistinctUntilChanged()
        //      .Select (c => neighborBombCount())
        .Subscribe (c => {
          //        watchEnvs();
        })
        .AddTo (this);


      model.visited
        .DistinctUntilChanged()
        .Subscribe (b =>  {
        })
        .AddTo (this);





    }
    get { return this.model; }
  }
}
