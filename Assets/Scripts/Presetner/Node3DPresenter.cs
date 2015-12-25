using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class Node3DPresenter : MonoBehaviour
{
  [SerializeField]
  GameObject[] walls;
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

  CompositeDisposable modelResources = new CompositeDisposable();
  Node model;
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

  public Node Model
  {
    set
    {
      this.model = value;

      modelResources.Clear();
      var mt = floor.GetComponent<Renderer>().material;
      mt.EnableKeyword("_EMISSION");

      model.AlertCount
        .DistinctUntilChanged()
        .Subscribe(c =>
        {
          alertText.text = c == 0 ? "" : c.ToString();
        })
        .AddTo(this);
      model.EnemyCount
        .DistinctUntilChanged()
        .Subscribe(c =>
        {
        })
        .AddTo(this);


      model.OnHere
        .CombineLatest(model.OnDest, (l, r) => l || r)
        .Subscribe(b => wallContainer.SetActive(b))
        .AddTo(this);
      model.OnHere
        .Subscribe(b =>
        {
          if(b)
          {
            //            sq.PrependInterval(.5f);
          }
          else
          {
          }
        })
        .AddTo(this);

      var deadend =
      model.Degree
        .Select(d => d == 1)
        .ToReactiveProperty();

      var hasEnemy =
        model.EnemyCount
        .Select(c => c > 0)
        .ToReactiveProperty();


      deadend
        .CombineLatest(model.HasItem, hasEnemy, model.AlertCount, model.isExit, (d, i, e, a, x) =>
      new Color(
          e ? 1 : (a > 0 ? .1f : 0),
          i ? 1 : (d ? 0f : 0),
          x ? 1 : 0))
        .Subscribe(c =>
        {
          mt.SetColor("_EmissionColor", c);
        })
        .AddTo(this);


      model.OnDest
        .Subscribe(b =>
        {
          if(b)
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
      model.IsVisited
        .Where(b => b)
        .DistinctUntilChanged()
        //      .Select (c => neighborBombCount())
        .Subscribe(c =>
        {
          //        watchEnvs();
        })
        .AddTo(this);


      model.IsVisited
        .DistinctUntilChanged()
        .Subscribe(b =>
        {
        })
        .AddTo(this);

      model.isExit
        .Subscribe(b =>
        {
          beacon.SetActive(b);
        });


      model.OnDestroy += modelDestoryHandler;
    }
    get { return this.model; }
  }
  void modelDestoryHandler(object sender, EventArgs e)
  {
    Destroy(gameObject);
  }
  void OnDestroy()
  {
    if(model != null)
    {
      model.OnDestroy -= modelDestoryHandler;
    }
  }
}
