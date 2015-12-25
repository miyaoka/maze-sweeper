using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
  [SerializeField]
  GameObject playerPrefab;
  [SerializeField]
  GameObject survivorDeadPrefab;
  [SerializeField]
  GameObject bloodPrefab;
  [SerializeField]
  GameObject player;
  public ReactiveProperty<IntVector2> CurrentCoords = new ReactiveProperty<IntVector2>();
  public ReactiveProperty<IntVector2> DestCoords = new ReactiveProperty<IntVector2>();
  public ReactiveProperty<int> Health = new ReactiveProperty<int>(5);

  Sequence sq;
  GraphManager gm;

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    gm = GraphManager.Instance;
  }
  void Start()
  {
  }
  public void MoveDir(Dirs dir)
  {
    if(sq != null)
    {
      if(sq.IsPlaying())
      {
        CurrentCoords.Value = DestCoords.Value;
      }
    }

    var sr = player.GetComponentInChildren<SpriteRenderer>();
    sr.flipX =
      dir == Dirs.East
      ? true
      : dir == Dirs.West
      ? false
      : sr.flipX;

    /*
    var edge = gm.graph.getNode(currentCoords.Value, dir);
    if (edge == null) {
      return;
    }
    */
    MovePos(CurrentCoords.Value + GraphModel.DirCoords[(int)dir]);
  }
  public void MovePos(IntVector2 dest)
  {
    var node = gm.VisitNode(dest);
    if(node == null)
    {
      return;
    }
    DestCoords.Value = dest;

    CameraManager.Instance.MovePos(dest);

    if(sq != null)
    {
      sq.Kill();
    }
    sq = DOTween.Sequence();

    AudioManager.door.Play();
    AudioManager.walk.PlayDelayed(.4f);

    player.GetComponentInChildren<Animator>().SetBool("isWalking", true);

    sq.Append(player.transform
      .DOLocalMove(gm.CoordsToVec3(dest), .8f)
      .SetEase(Ease.InOutQuad)
    );
    sq.OnKill(() =>
    {
      CurrentCoords.Value = dest;

      player.GetComponentInChildren<Animator>().SetBool("isWalking", false);


      var ec = node.EnemyCount.Value;
      if(ec > 0)
      {
        Debug.Log("enemy:" + ec);
        Health.Value -= ec;
        gm.ClearNodeEnemy(node);

        AudioManager.maleScream.Play();
        while(ec-- > 0)
        {
          createDead(dest);
        }

      }
      if(node.isExit.Value)
      {
        GameManager.Instance.onExit();
      }
      GameManager.Instance.alertCount.Value = node.AlertCount.Value = gm.graph.ScanEnemies(dest);
    });


  }

  public void SetPos(IntVector2 dest)
  {
    DestCoords.Value = dest;
    CurrentCoords.Value = dest;
    CameraManager.Instance.MovePos(dest);
    player.transform.localPosition = gm.CoordsToVec3(dest);
    gm.VisitNode(dest);
  }
  void createDead(IntVector2 dest)
  {
    //    var diff = Random.insideUnitCircle * 2f;
    var range = 2f;
    var dead = Instantiate(
      survivorDeadPrefab,
      gm.CoordsToVec3(dest) + new Vector3(Random.Range(-range, range), 0.1f, Random.Range(-range, range)),
      Quaternion.Euler(new Vector3(20f, Random.Range(0, 360f), 0))
    ) as GameObject;

    gm.AddToView(dead);
    dead.transform.DOLocalRotate(
      new Vector3(90, dead.transform.rotation.eulerAngles.y, 0), Random.Range(.3f, .6f)
    ).SetEase(Ease.InCirc);
  }
}
