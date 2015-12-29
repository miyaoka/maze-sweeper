using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class NodePresenter : MonoBehaviour
{
  [SerializeField]
  Light roomLight;
  [SerializeField]
  GameObject alert;
  [SerializeField]
  GameObject wallContainer;
  [SerializeField]
  GameObject tiles;
  [SerializeField]
  GameObject beacon;
  [SerializeField]
  GameObject floor;
  [SerializeField]
  GameObject interiorPrefab;
  [SerializeField]
  GameObject wallPrefab;


  Node node;
  Tweener lightTw;
  Text alertText;
  float lightMax = 1.0f;
  float lightMin = 0f;
  void Awake()
  {
    roomLight.intensity = 0;
    lightTw = roomLight.DOIntensity(0, 0);//.SetLoops (10, LoopType.Yoyo);
    roomLight.enabled = false;

    tiles.SetActive(false);
    alertText = alert.GetComponent<Text>();
  }

  public Node Node
  {
    set
    {
      this.node = value;

      var mt = floor.GetComponent<Renderer>().material;
      mt.EnableKeyword("_EMISSION");

      node.AlertCount
        .DistinctUntilChanged()
        .Subscribe(c =>
        {
          alertText.text = c == 0 ? "" : c.ToString();
        })
        .AddTo(this);
      node.EnemyCount
        .DistinctUntilChanged()
        .Subscribe(c =>
        {
        })
        .AddTo(this);


      node.OnHere
        .CombineLatest(node.OnDest, (l, r) => l || r)
        .Subscribe(b => wallContainer.SetActive(b))
        .AddTo(this);
      node.OnHere
        .Subscribe(b =>
        {
          if (b)
          {
            //            sq.PrependInterval(.5f);
          }
          else
          {
          }
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
        .CombineLatest(node.HasItem, hasEnemy, node.AlertCount, node.isExit, (d, i, e, a, x) =>
      new Color(
          e ? 1 : (a > 0 ? .1f : 0),
          i ? 1 : (d ? 0f : 0),
          x ? 1 : 0))
        .Subscribe(c =>
        {
          mt.SetColor("_EmissionColor", c);
        })
        .AddTo(this);


      node.OnDest
        .Subscribe(b =>
        {
          if (b)
          {
            tiles.SetActive(b);

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
                tiles.SetActive(b);

              });
          }
        })
        .AddTo(this);

      node.isExit
        .Subscribe(b =>
        {
          beacon.SetActive(b);
        });

      for(var i = 0; i < 4; i++)
      {
        var w = addWall(i);
        w.index = i;
        w.Node = node;
        if(Random.value < .3f)
        {
          addInterior(i);
        }
      }

      node.OnDestroy += modelDestoryHandler;
    }
    get { return this.node; }
  }
  WallPresenter addWall(int dir)
  {
    var wallThick = .4f;
    var halfRoomSize = 5f + wallThick * .5f;
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
      Quaternion.Euler(new Vector3(0,dir * -90, 0))
      ) as GameObject;
    obj.transform.SetParent(wallContainer.transform, false);
    return obj.GetComponent<WallPresenter>();
  }
  //dir 0-3
  void addInterior(int dir)
  {
    var scale = new Vector3(Random.Range(.5f, 3f), Random.Range(.5f, 4f), Random.Range(.5f, 3f));
    var halfRoomSize = 5f;
    var pos = new Vector3(halfRoomSize - scale.x * .5f, scale.y * .5f, halfRoomSize - scale.z * .5f);
    pos.x *= dir % 2 == 0 ? 1 : -1;
    pos.z *= dir / 2 > 0 ? 1 : -1;
    var obj = Instantiate(interiorPrefab, pos, Quaternion.identity) as GameObject;
    obj.transform.SetParent(wallContainer.transform, false);
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
