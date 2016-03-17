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
  Light roomLight;
  [SerializeField]
  Text alertCountText;
  [SerializeField]
  Text enemyCountText;
  [SerializeField]
  Transform interiorContainer;
  [SerializeField]
  GameObject tiles;
  [SerializeField]
  GameObject beacon;
  [SerializeField]
  GameObject floor;
  [SerializeField]
  GameObject interiorPrefab;
  [SerializeField]
  GameObject firePrefab;
  [SerializeField]
  GameObject grenadeTargetBtnPrefab;
  [SerializeField]
  GameObject survivorPrefab;
  [SerializeField]
  Image alertFloor;

  Node node;
  Tweener lightTw;
  float lightMaxRoom = 2f;
  float lightMaxPassage = 2.5f;
  float lightMin = 0f;
  Color unvisitedFloorColor = new Color(.25f, .25f, .3f);
  float grenadeTargetBtnHeight = 2f;
  float alertFloorAlpha = .7f;

  GameObject sv = null;


  void Awake()
  {
    roomLight.intensity = 0;
    lightTw = roomLight.DOIntensity(0, 0);//.SetLoops (10, LoopType.Yoyo);
    roomLight.enabled = false;

    tiles.SetActive(false);

    var c = alertFloor.color;
    c.a = 0;
    alertFloor.color = c;
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


      node.IsScanned
        .Subscribe(s =>
        {
          alertCountText.enabled = enemyCountText.enabled = s;
        })
        .AddTo(this);

      node.AlertCount
        .CombineLatest(node.EnemyCount, (a,e) => e > 0 ? 0 : a)
        .Select(c => c == 0 ? "" : c.ToString())
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
          alertFloor.DOFade(exist ? alertFloorAlpha : 0, 1f)
          .SetEase(exist ? Ease.InQuad : Ease.OutQuad);

        });

      node.EnemyCount
        .CombineLatest(node.OnDest, (e, d) => d ? 0 : e)
        .Select(c => c == 0 ? "" : c.ToString())
        .SubscribeToText(enemyCountText)
        .AddTo(this);

      node.IsScanned
        .CombineLatest(LevelManager.Instance.CurrentView, (s, v) => s && v != ViewState.Map)
        .Subscribe(b => {
          interiorContainer
          .GetComponentsInChildren<MeshRenderer>()
          .ToList()
          .ForEach(m => m.enabled = b);

          tiles.SetActive(b);
        })
        .AddTo(this);

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

      var roomFloorColorValue = .9f;
      var passageFloorColorValue = .6f;

      //floor color
      node.IsVisited
        .Select(v => v ? (node.IsRoom ? roomFloorColorValue : passageFloorColorValue) : 0)
        .Select(v => Color.HSVToRGB(0, 0, v))
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
        .Select(v => LayerMask.NameToLayer(v ? "Default" : "UnVisited"))
        .Subscribe(l => floor.layer = l)
        .AddTo(this);



/*
      node.IsScanned
        .Select(v => v ? initFloorColor : unvisitedFloorColor)
        .Subscribe(c => mt.color = c)
        .AddTo(this);
*/

      node.OnDest
        .Subscribe(b =>
        {
          if (b)
          {
            roomLight.enabled = true;
            lightTw.Kill();
            lightTw = roomLight.DOIntensity(node.IsRoom ? lightMaxRoom : lightMaxPassage, Random.Range(.4f, .6f)).SetEase(Ease.InQuad);
          }
          else
          {
            lightTw.Kill();
            lightTw = roomLight.DOIntensity(lightMin, Random.Range(.3f, .5f)).SetEase(Ease.OutQuad).SetDelay(.5f)
              .OnComplete(() =>
              {
                roomLight.enabled = false;
              });
          }
        })
        .AddTo(this);

      node.isExit
        .Subscribe(b =>
        {
          beacon.SetActive(b);
        });

      node.IsScanned
        .CombineLatest(node.HasFire, (l,r) => l && r)
        .Where(b => b)
        .Subscribe(_ =>
        {
          var fire = Instantiate(firePrefab);
          var ps = fire.GetComponent<ParticleSystem>();
          ps.startSize = Random.Range(6f, 12f);
          ps.startLifetime = Mathf.Pow(ps.startSize, .5f);

          var halfRoomSize = 4f;
          var pos = new Vector3(Random.Range(-halfRoomSize, halfRoomSize), .6f, Random.Range(-halfRoomSize, halfRoomSize));
          fire.transform.position = pos;
          fire.transform.SetParent(interiorContainer, false);

        })
        .AddTo(this);

      node.IsScanned
        .CombineLatest(node.HasRescuee, (l, r) => l && r)
        .Where(b => b)
        .Subscribe(_ =>
        {
          sv = Instantiate(survivorPrefab);

          //22.5deg-67.5deg
          var radian = (Random.Range(0, 1f/4f) + 1f/8f + Random.Range(0, 4) * .5f) * Mathf.PI;
          var radius = Random.Range(2f, 4f);

          var pos = new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian)) * radius;

          sv.transform.position = pos;
          sv.transform.SetParent(transform, false);
          var sp = sv.GetComponent<SurvivorPresenter>();
          sp.body.transform.LookAt(transform);
//          sp.GetComponentInChildren<Animator>().enabled = false;
        })
        .AddTo(this);

      node.HasRescuee
        .Subscribe(r =>
        {
          if (!r && sv != null)
          {
            Destroy(sv);
          }
        })
        .AddTo(this);


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
  /*
  WallPresenter addWall(int dir)
  {
    var halfRoomSize = 5f;
    var pos = Vector3.zero;
    switch (dir)
    {
      case 0:
        pos.x = halfRoomSize;
        break;
      case 1:
        pos.z = halfRoomSize;
        break;
      case 2:
        pos.x = -halfRoomSize;
        break;
      case 3:
        pos.z = -halfRoomSize;
        break;
    }
    var obj = Instantiate(
      wallPrefab,
      pos,
      Quaternion.Euler(new Vector3(0, dir * -90, 0))
      ) as GameObject;
    obj.transform.SetParent(wallContainer, false);
    return obj.GetComponent<WallPresenter>();
  }
  */
  //dir 0-3
  void addInterior(int dir)
  {
    var scale = new Vector3(Random.Range(.5f, 3f), Random.Range(.5f, 4f), Random.Range(.5f, 3f));
    var halfRoomSize = 5f;
    var pos = new Vector3(halfRoomSize - scale.x * .5f, scale.y * .5f, halfRoomSize - scale.z * .5f);
    pos.x *= dir % 2 == 0 ? 1 : -1;
    pos.z *= dir / 2 > 0 ? 1 : -1;
    var obj = Instantiate(interiorPrefab, pos, Quaternion.identity) as GameObject;
    obj.transform.SetParent(interiorContainer, false);
    obj.transform.localScale = scale;
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
