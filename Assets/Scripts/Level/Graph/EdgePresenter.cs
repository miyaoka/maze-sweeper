using DG.Tweening;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Random = UnityEngine.Random;
public class EdgePresenter : MonoBehaviour
{
  [SerializeField]
  GameObject door;
  [SerializeField]
  GameObject brokenWall;
  [SerializeField]
  GameObject explosionPrefab;

  void Awake()
  {
    brokenWall.SetActive(false);
  }
  private Edge edge;
  public Edge Edge
  {
    set
    {
      this.edge = value;

      edge
        .isOpened
        .Subscribe(b => {
          if (!b)
          {
            door.SetActive(true);
            return;
          }

          AudioManager.Instance.Play(AudioName.Door);

          door.transform
          .DOLocalMoveY(-2, .3f)
          .OnComplete(() =>
          {
            door.SetActive(false);
          })
          .SetEase(Ease.InQuad);

          brokenWall.transform
          .DOLocalMoveY(-.5f, .3f)
          .OnComplete(() =>
          {
            brokenWall.SetActive(false);
          })
          .SetEase(Ease.InCirc);
        })
        .AddTo(this);

      edge.OnDestroy += modelDestoryHandler;
    }
    get { return this.edge; }
  }
  public void breach()
  {
    AudioManager.Instance.Play(AudioName.Breach);
    var explosionObj = Instantiate(
      explosionPrefab
      ) as GameObject;
    explosionObj.transform.SetParent(transform, false);
    Destroy(explosionObj, 3f);

    door.SetActive(false);
    brokenWall.SetActive(true);
    var t = brokenWall.transform;
    var seq = DOTween.Sequence();
    seq.Append(
      t.DOLocalRotate(new Vector3(Random.Range(-5f,5f), Random.Range(-10f,10f), -90), .3f)
      .SetEase(Ease.InCubic)
    );
    seq.Join(
      t.DOLocalMoveX(Random.Range(.5f, 1.0f), .2f)
      .SetEase(Ease.OutCubic)
    );
    seq.Join(
      t.DOLocalMoveY(.5f, .3f)
      .SetEase(Ease.InCubic)
    );
  }
  void modelDestoryHandler(object sender, EventArgs e)
  {
    Destroy(gameObject);
  }

  void OnDestroy()
  {
    if(edge != null)
    {
      edge.OnDestroy -= modelDestoryHandler;
    }
  }
}
