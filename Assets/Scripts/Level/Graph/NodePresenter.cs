using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class NodePresenter : MonoBehaviour
{
  [SerializeField]
  Text alertCountText;
  [SerializeField]
  GameObject floor;
  [SerializeField]
  Transform interiorContainer;


  [SerializeField]
  GameObject grenadeTargetBtnPrefab;
  [SerializeField]
  GameObject interiorPrefab;

  [SerializeField]
  Texture floorTexture1;
  [SerializeField]
  Texture floorTexture2;

  Node node;
  Color unvisitedFloorColor = new Color(.25f, .25f, .3f);
  float grenadeTargetBtnHeight = 2f;
  float alertFloorAlpha = .7f;

  GameObject sv = null;


  void Awake()
  {
  }

  public Node Node
  {
    set
    {
      this.node = value;
      node.OnDestroy += modelDestoryHandler;

      var graph = GraphManager.Instance;

      var mt = floor.GetComponent<Renderer>().material;
      mt.EnableKeyword("_EMISSION");
      mt.color = Color.clear;

      node
        .HasItem
        .Subscribe(item =>
        {
          if(!item)
          {
            foreach(Transform t in interiorContainer)
            {
              Destroy(t.gameObject);
            }
            return;
          }
          addItemInterior();
          /*
          var dirList = new List<int>{ 0, 1, 2, 3};
          dirList.Sort((a, b) => Random.value < .5f ? -1 : 1);

          dirList
          .Take(Random.Range(1, dirList.Count))
          .ToList()
          .ForEach(i => addInterior(i));
          */

        });



      node.AlertCount
        .CombineLatest(node.EnemyCount, node.IsVisited, (a, e, v) => !v || e > 0 ? 0 : a)
        .Select(c => c <= 0 ? "" : c.ToString())
        .SubscribeToText(alertCountText)
        .AddTo(this);

      Tween alertFloorTw = null;
      node.AlertCount
        .CombineLatest(node.EnemyCount, (a, e) => e > 0 ? 0 : a)
        .Select(c => c > 0)
        .Subscribe(exist =>
        {
          if(alertFloorTw != null)
          {
            alertFloorTw.Kill();
          }

          alertFloorTw =
          mt.DOFade(exist ? alertFloorAlpha : 0, 1f)
          .SetEase(exist ? Ease.InQuad : Ease.OutQuad);

        });

      var hasEnemy =
        node.EnemyCount
        .Select(c => c > 0)
        .ToReactiveProperty();
      var hasAlert =
        node.AlertCount
        .Select(c => c > 0)
        .ToReactiveProperty();


      //emission colors
      var enemyColor = new Color(1, 0, 0);
      var alertColor = new Color(.5f, .2f, .2f);
      var energyColor = new Color(.0f, .6f, .0f);
      var rescueeColor = new Color(1f, 1f, 1f);
      var itemColor = new Color(.3f, .3f, .3f);
      var noneColor = Color.clear;

      Tween textScaleTw = null;
      node.AlertCount
        .CombineLatest(node.IsVisited, (a,v) => v ? a : 0)
        .Where(c => c > 0)
        .Subscribe(c =>
        {
          alertCountText.transform.localScale = Vector3.zero;

          if(textScaleTw != null)
          {
            textScaleTw.Kill();
          }
          textScaleTw = alertCountText.transform
          .DOScale(Vector3.one, 1f)
          .SetEase(Ease.OutElastic)
          .SetDelay(.2f);
        })
         .AddTo(this);

      /*
      Tween emitColorTw = null; 
      //floor alert color
      node.HasEnergy
        .CombineLatest(node.HasItem, node.HasRescuee, (e, i, r) => e ? energyColor : i ? itemColor : r ? rescueeColor : noneColor)
        .CombineLatest(node.IsScanned, hasEnemy, hasAlert, (i, s, e, a) =>
        s ? (e ? enemyColor : i != noneColor ? i : (a ? alertColor : noneColor))
        : i)
        .Subscribe(endColor =>
        {
          var isClear = endColor == Color.clear;
          var c = mt.GetColor("_EmissionColor");
          if (emitColorTw != null)
          {
            emitColorTw.Kill();
          }
          emitColorTw = DOTween
          .To(() => c, x => c = x, endColor, isClear ? .5f : 1f)
          .SetEase(isClear ? Ease.OutQuad : Ease.InQuad)
          .SetDelay(.2f)
          .OnUpdate(() => mt.SetColor("_EmissionColor", c));
          
        })
        .AddTo(this);
        */

      Tween floorColorTw = null;

      var roomFloorColorValue = 1f;
      var passageFloorColorValue = .8f;

      var roomFloorColor = Color.HSVToRGB(.1f, .5f, 1f);
      var passageFloorColor = Color.HSVToRGB(.3f, .5f, .8f);

      //floor color
      node.IsVisited
        .Select(v => v ? (node.IsRoom ? roomFloorColor : passageFloorColor) : Color.HSVToRGB(.0f, .0f, .0f) )
//        .Select(v => Color.HSVToRGB(0.3f, 0.5f, v))
        .Subscribe(c =>
        {
          if(floorColorTw != null)
          {
            floorColorTw.Kill();
          }
          floorColorTw = mt.DOColor(c, 1f).SetEase(Ease.OutQuad);
        })
        .AddTo(this);

      node.IsVisited
        .Subscribe(v => {
          mt.mainTexture = v ? floorTexture1 : floorTexture2;
        })
        .AddTo(this);


      /*
      node.IsVisited
        .Select(v => LayerMask.NameToLayer(v ? "Default" : "UnVisited"))
        .Subscribe(l => floor.layer = l)
        .AddTo(this);
        */




      //sensor btn
      node.OnDest
        .CombineLatest(LevelManager.Instance.IsSelectedGrenade, (l, r) => l && r)
        .Where(b => b)
        .Subscribe(_ =>
        {
          node.EdgeArray
          .Select((v, i) => new { Value = v, Index = i })
          .ToList()
          .Where(e => e.Value != null)
          .ToList()
          .ForEach(e =>
          {
            var coords = node.Coords + Graph.NextGridCoords[e.Index];
            var nn = graph.graph.GetNode(coords);
            if(!nn.IsScanned.Value)
            {
              var btn = Instantiate(grenadeTargetBtnPrefab);
              btn.transform.localPosition = graph.CoordsToVec3(coords) + new Vector3(0, grenadeTargetBtnHeight, 0);
              btn.GetComponent<GrenadeTargetBtnPresenter>().Coords = coords;
              graph.AddToView(btn);
            }
          });
        })
        .AddTo(this);

    }
    get { return this.node; }
  }

  void addItemInterior()
  {
    var scale = new Vector3(Random.Range(1f, 3f), Random.Range(1f, 4f), Random.Range(1f, 3f));
    var pos = new Vector3(0, scale.y * .5f, 2f);
    var obj = Instantiate(interiorPrefab, pos, Quaternion.identity) as GameObject;
    obj.transform.SetParent(interiorContainer, false);
    obj.transform.localScale = scale;

  }


  void modelDestoryHandler(object sender, EventArgs e)
  {
    Destroy(gameObject);
  }
  void OnDestroy()
  {
    if (node != null)
    {
      node.OnDestroy -= modelDestoryHandler;
    }
  }
}
