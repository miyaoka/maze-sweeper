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
  /*
  GameObject _player;
  GameObject player {
    get {
      if (_player == null) {
        _player = Instantiate (playerPrefab);
        gm.addToViewContainer (_player);
        _player.transform.rotation = Quaternion.Euler (new Vector3 (20, 0, 0));
      }
      return _player;
    }
  }
  */
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
    //    sq = DOTween.Sequence ();
    //    movePos (currentCoords.Value);
    //    movePos (new IntVector2 (3, 3));

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
    MovePos(CurrentCoords.Value + GraphManager.DirCoords[(int)dir]);
  }
  public void MovePos(IntVector2 dest)
  {
    var node = gm.graph.GetNode(dest);
    if(node == null)
    {
      return;
    }
    DestCoords.Value = dest;

    if(sq != null)
    {
      if(sq.IsPlaying())
      {
        //        return;
      }
    }

    /*
    if (!node.visited.Value) {
      gm.createNodeView (node);
      node.visited.Value = true;

      var edges = gm.getAllEdgesFromNode (node.coords);
      foreach (var e in edges) {
        if (!e.visited) {
          e.visited = true;
          gm.createEdgeView (e);
        }
      }
    }
    */

    CameraManager.Instance.movePos(dest);

    if(sq != null)
    {
      sq.Kill();
    }
    sq = DOTween.Sequence();

    AudioManager.door.Play();
    AudioManager.walk.PlayDelayed(.4f);
    //    AudioManager.enemyDetect.Play ();

    player.GetComponentInChildren<Animator>().SetBool("isWalking", true);

    sq.Append(player.transform
      .DOLocalMove(gm.CoordsToVec3(dest), .8f)
      .SetEase(Ease.OutQuad)
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

        foreach(var n in gm.graph.Neighbors(dest))
        {
          if(n == node)
          {
            continue;
          }
          //TODO:未探索nodeだとマイナスになる
          //0に補正するか0以下は表示しないか
          n.AlertCount.Value = Mathf.Max(0, n.AlertCount.Value - ec);
        }

        node.EnemyCount.Value = 0;
        //          ec -= 1;

        //ランダム位置にワープ
        //TODO: 同じ位置に飛ばないようにする
        if(0 < ec)
        {
          //                      nodeList[Random.Range(0, nodeList.Count)].enemyCount.Value += ec;
        }

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
  void createDead(IntVector2 dest)
  {
    //    var diff = Random.insideUnitCircle * 2f;
    var range = 2f;
    var dead = Instantiate(
      survivorDeadPrefab,
      gm.CoordsToVec3(dest) + new Vector3(Random.Range(-range, range), 0.1f, Random.Range(-range, range)),
      Quaternion.Euler(new Vector3(20f, Random.Range(0, 360f), 0))
    ) as GameObject;

    //    gm.addToViewContainer(dead);
    dead.transform.DOLocalRotate(
      new Vector3(90, dead.transform.rotation.eulerAngles.y, 0), Random.Range(.3f, .6f)
    ).SetEase(Ease.InCirc);
  }
}
