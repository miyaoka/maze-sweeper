using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using DG.Tweening;

public class Node3DPresenter : MonoBehaviour {
  [SerializeField] GameObject wallN;
  [SerializeField] GameObject wallE;
  [SerializeField] GameObject wallW;
  [SerializeField] GameObject wallS;
  [SerializeField] Light light;
  [SerializeField] GameObject alert;
  [SerializeField] GameObject walls;
  [SerializeField] GameObject tiles;

  CompositeDisposable modelResources = new CompositeDisposable();
  float fadeIn = .5f;
  float fadeOut = .1f;
  private NodeModel model;
  Sequence sq;
  Sequence wallSq;
  Tweener lightTw;
  TextMesh alertText;
  void Awake(){
    
    light.intensity = 0;
    lightTw = light.DOIntensity (0, 0);//.SetLoops (10, LoopType.Yoyo);
    light.enabled = false;

    tiles.SetActive (false);
    alertText = alert.GetComponent<TextMesh> ();
  }
  public NodeModel Model
  {
    set { 
      this.model = value; 
      sq = DOTween.Sequence ();

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

      var activeFloorColor = new Color (Random.Range (.7f, .9f), Random.Range (.7f, .9f),Random.Range (.7f, .9f));
      var visitedFloorColor = new Color (.2f, .2f, .2f);
      activeFloorColor = new Color(.8f,.8f,.8f);

      model.onHere
        .CombineLatest (model.onDest, (l, r) => l || r)
        .Subscribe (b => walls.SetActive (b))
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

            light.enabled = true;
            lightTw.Kill();
            lightTw = light.DOIntensity (1.0f, Random.Range(.3f, .5f)).SetEase (Ease.InBounce);
          }
          else{
            lightTw.Kill();
            lightTw = light.DOIntensity (0.0f, Random.Range(.3f, .5f)).SetEase (Ease.OutQuad).SetDelay(.5f)
              .OnComplete(() =>{
                light.enabled = false;
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
