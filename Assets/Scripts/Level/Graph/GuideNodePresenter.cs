using UnityEngine;
using UniRx;
using DG.Tweening;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class GuideNodePresenter : MonoBehaviour
{
  [SerializeField]
  ParticleSystem particle;

  Node node;
  void Awake()
  {
    particle.Stop();
  }
  public Node Node
  {
    set
    {
      this.node = value;
      node.OnDestroy += modelDestoryHandler;

      var hueSpan = .12f;
      var initRate = .5f;
      var rateSpan = .25f;

      var ds =
      node.DangerLevel
        .Subscribe(d =>
        {
          var emit = particle.emission;
          if (d == 0)
          {

            emit.enabled = false;
            particle.loop = false;
            particle.playbackSpeed = 2;
          }
          else
          {
            var lv = Mathf.Min((d - 1) / 4f, 1);

            //hue: yellow to red
            var cl = Color.HSVToRGB((1 - lv) * hueSpan, 1f, 1f);
            emit.enabled = true;
            emit.rate = new ParticleSystem.MinMaxCurve(initRate + lv * rateSpan);

            particle.loop = true;
            particle.startColor = cl;
            particle.playbackSpeed = 1f + lv * .2f;
            particle.Play();

          }
        });

      node
        .IsScanned
        .Where(s => s)
        .Subscribe(_ =>
        {
          particle.Stop();
          particle.Clear();
          ds.Dispose();
        })
        .AddTo(this);

    }
    get { return this.node; }
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
