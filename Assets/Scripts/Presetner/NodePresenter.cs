﻿using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class NodePresenter : MonoBehaviour
{
  [SerializeField]
  Light roomLight;
  [SerializeField]
  Text alertCountText;
  [SerializeField]
  Text enemyCountText;
  [SerializeField]
  Transform wallContainer;
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
  GameObject sensorTargetBtnPrefab;

  Node node;
  Tweener lightTw;
  float lightMax = 1.8f;
  float lightMin = 0f;
  Color unvisitedFloorColor = new Color(.25f, .25f, .3f);
  void Awake()
  {
    roomLight.intensity = 0;
    lightTw = roomLight.DOIntensity(0, 0);//.SetLoops (10, LoopType.Yoyo);
    roomLight.enabled = false;

    tiles.SetActive(false);
  }

  public Node Node
  {
    set
    {
      this.node = value;
      node.HasView.Value = true;
      var graph = GraphManager.Instance;

      var mt = floor.GetComponent<Renderer>().material;
      mt.EnableKeyword("_EMISSION");
      var initFloorColor = mt.color;

      for (var i = 0; i < 4; i++)
      {
        if (Random.value < .3f)
        {
          addInterior(i);
        }
      }


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

      node.EnemyCount
        .CombineLatest(node.OnDest, (e, d) => d ? 0 : e)
        .Select(c => c == 0 ? "" : c.ToString())
        .SubscribeToText(enemyCountText)
        .AddTo(this);

      node.IsScanned
        .CombineLatest(RoundManager.Instance.IsMapView, (s, m) => s && !m)
        .Subscribe(b => {
          interiorContainer
          .GetComponentsInChildren<MeshRenderer>()
          .ToList()
          .ForEach(m => m.enabled = b);

          tiles.SetActive(b);
        })
        .AddTo(this);

      var deadend =
      node.Degree
        .Select(d => d == 1)
        .ToReactiveProperty();

      var hasEnemy =
        node.EnemyCount
        .Select(c => c > 0)
        .ToReactiveProperty();


      deadend
        .CombineLatest(hasEnemy, node.AlertCount, node.IsScanned, (d, e, a, v) =>
          new Color(!v ? 0 : e ? 1 : (a > 0 ? .2f : 0), 0, 0))
        .CombineLatest(node.HasItem, (c, i) => i ? new Color(0,1,0) : c)
        .Subscribe(c =>
        {
          mt.SetColor("_EmissionColor", c);
        })
        .AddTo(this);

      node.IsScanned
        .Select(v => v ? initFloorColor : unvisitedFloorColor)
        .Subscribe(c => mt.color = c)
        .AddTo(this);


      node.OnDest
        .Subscribe(b =>
        {
          if (b)
          {
            roomLight.enabled = true;
            lightTw.Kill();
            lightTw = roomLight.DOIntensity(lightMax, Random.Range(.4f, .6f)).SetEase(Ease.InQuad);
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

      node.HasFire
        .Where(f => f)
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

      node.OnDest
        .CombineLatest(RoundManager.Instance.IsSelectedSensor, (l, r) => l && r)
        .Where(b => b)
        .Subscribe(_ =>
        {
          GraphModel.DirCoords.ToList().ForEach(c =>
          {
            var coords = node.Coords + c;
            var n = graph.graph.GetNode(coords);
            if(n == null || !n.IsScanned.Value)
            {
              var btn = Instantiate(sensorTargetBtnPrefab);
              btn.transform.localPosition = graph.CoordsToVec3(coords);
              btn.GetComponent<SensorTargetBtnPresenter>().Coords = coords;
              graph.AddToView(btn);
            }
          });
        })
        .AddTo(this);

      /*
      node.Degree
        .Subscribe(_ =>
        {
          foreach(Transform t in wallContainer)
          {
            Destroy(t.gameObject);
          }
          for (var i = 0; i < 4; i++)
          {
            var e = node.EdgeArray[i];
            if (e == null)
            {
              var w = addWall(i);
              w.Dir = i;
              w.NodeModel = node;
            }
          }
        }).AddTo(this);
        */


      node.OnDestroy += modelDestoryHandler;
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
