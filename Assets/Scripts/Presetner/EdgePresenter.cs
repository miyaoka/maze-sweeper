﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using DG.Tweening;
using System;
public class EdgePresenter : MonoBehaviour{
  [SerializeField] Image edgeImage;
  [SerializeField] CanvasGroup cg;


  CompositeDisposable typeResources = new CompositeDisposable();
  CompositeDisposable modelResources = new CompositeDisposable();
  Tweener t;

  private Edge model;
  public Edge Model
  {
    set { 
      this.model = value; 

      modelResources.Clear ();

      //change image by edgetype
      /*
      model.type
        .Where(t => t != null)
        .Subscribe(t => {
          typeResources.Clear();
          t.isPassable
            .Subscribe(p => edgeImage.gameObject.SetActive(p))
            .AddTo(typeResources);
        })
        .AddTo(this); 
        */


      //player is on the one of nodes
      model.sourceNode.onHere
        .CombineLatest(model.targetNode.onHere, (l,r) => l | r)
        .Subscribe(b => {
          if(t != null){
            t.Kill();
          }
          t = edgeImage.DOColor(b ? new Color(.8f, .8f, .8f) : new Color(.4f, .4f, .4f), b ? 1f : .2f).SetEase(Ease.OutQuad);

        })
        .AddTo(this);

      //visited one of nodes
      model.sourceNode.visited
        .CombineLatest(model.targetNode.visited, (l,r) => l | r)
        .Subscribe(b => {
          cg.DOFade(b ? 1 : 0, b ? 1 : 0).SetEase(Ease.OutQuad);
        })
        .AddTo(this);

      model.OnDestroy += modelDestoryHandler;
    }
    get { return this.model; }
  }
  void modelDestoryHandler (object sender, EventArgs e)
  {
    model.OnDestroy -= modelDestoryHandler;
    Destroy (gameObject);
  }

  void OnDestroy()
  {
    typeResources.Dispose ();
  }
}
