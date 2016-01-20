using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using System.Linq;
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
  public ReactiveProperty<int> Health = new ReactiveProperty<int>();

  Sequence sq;
  GraphManager gm;

  void Awake()
  {
    if (this != Instance)
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
    if (sq != null)
    {
      if (sq.IsPlaying())
      {
        CurrentCoords.Value = DestCoords.Value;
      }
    }

    player.transform.DORotate(new Vector3(0, (int)dir * -90 + 90, 0), .5f).SetEase(Ease.InOutQuad);

    /*
    var sr = player.GetComponentInChildren<SpriteRenderer>();

    //default is flipped
    sr.flipX =
      dir == Dirs.East
      ? false
      : dir == Dirs.West
      ? true
      : sr.flipX;
      */
    var node = gm.graph.GetNode(CurrentCoords.Value);
    var edge = node.EdgeArray[(int)dir];
    if (edge == null) {
      return;
    }

    edge.isOpened.Value = true;

    MovePos(CurrentCoords.Value + GraphModel.DirCoords[(int)dir]);
  }
  public void MovePos(IntVector2 dest, bool noAnim = false)
  {
    GameManager.Instance.OnBomb.Value = false;
    var node = gm.ShowNode(dest);
    if (node == null)
    {
      return;
    }
    DestCoords.Value = dest;
    CameraManager.Instance.MovePos(dest);
    gm.ScanEnemies(node);

    //look-ahead
    /*
    node.EdgeArray
      .Select((v, i) => new { Value = v, Index = i })
      .Where(e => e.Value != null)
      .ToList()
      .ForEach(e => {
        gm.ShowNode(node.Coords + GraphModel.DirCoords[e.Index], false);
      });
      */

    if (noAnim)
    {
      player.transform.localPosition = gm.CoordsToVec3(dest);
      onMoved(node);
      return;
    }

    //if already moving, force complete.
    if (sq != null)
    {
      sq.Kill();
    }
    sq = DOTween.Sequence();
    startWalk();
    sq.Append(player.transform
      .DOLocalMove(gm.CoordsToVec3(dest), 1f)
      .SetEase(Ease.InOutQuad)
      .OnComplete(stopWalk)
    );
    sq.OnKill(() =>
    {
      onMoved(node);
    });
  }

  private void onMoved(Node node)
  {
    CurrentCoords.Value = node.Coords;
    var ec = node.EnemyCount.Value;
    if (ec > 0)
    {
      Debug.Log("enemy:" + ec);
//      Health.Value -= ec;
      SurvivorManager.Instance.AddDamages(ec);

      gm.ClearNodeEnemy(node);
      gm.ScanEnemies(node);

      AudioManager.MaleScream.Play();
      while (ec-- > 0)
      {
        createDead(node.Coords);
      }

    }
    if (gm.isExit(node))
    {
      GameManager.Instance.onExit();
    }
    if (node.HasItem.Value)
    {
      AudioManager.Powerup.Play();
      Health.Value += 1;
      node.HasItem.Value = false;
      GameManager.Instance.AddTime();
    }


  }

  private void stopWalk()
  {
    player.GetComponentInChildren<Animator>().SetBool("IsWalking", false);
    AudioManager.Instance.StopLoop(AudioManager.Walk);
  }

  private void startWalk()
  {
    //    AudioManager.door.Play();
    player.GetComponentInChildren<Animator>().SetBool("IsWalking", true);
    AudioManager.Instance.PlayLoop(AudioManager.Walk);
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
