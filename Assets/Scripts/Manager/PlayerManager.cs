using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>{
  [SerializeField] GameObject playerPrefab;
  [SerializeField] GameObject survivorDeadPrefab;
  [SerializeField] GameObject bloodPrefab;
  public ReactiveProperty<IntVector2> currentCoords = new ReactiveProperty<IntVector2> ();
  public ReactiveProperty<IntVector2> destCoords = new ReactiveProperty<IntVector2> ();
  Sequence sq;
  public ReactiveProperty<int> health = new ReactiveProperty<int>(5);

  GraphManager gm;
  [SerializeField] GameObject player;
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
  void Awake ()
  {
    if (this != Instance) {
      Destroy (this);
      return;
    }
    gm = GraphManager.Instance;
  }
  void Start () {
    //    sq = DOTween.Sequence ();
    //    movePos (currentCoords.Value);
    //    movePos (new IntVector2 (3, 3));

  }
  public void moveDir(Dirs dir)
  {
    if (sq != null){
      if (sq.IsPlaying ()) {
        currentCoords.Value = destCoords.Value;
      }
    }

    var sr = player.GetComponentInChildren<SpriteRenderer> ();
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
    movePos (currentCoords.Value + GraphManager.dirCoords[(int)dir]);
  } 
  public void movePos(IntVector2 dest){
    var node = gm.graph.getNode(dest);
    if (node == null) {
      return;
    }
    destCoords.Value = dest;

    if (sq != null){
      if (sq.IsPlaying ()) {
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

    CameraManager.Instance.movePos (dest);

    if (sq != null) {
      sq.Kill ();
    }
    sq = DOTween.Sequence();

    AudioManager.door.Play ();
    AudioManager.walk.PlayDelayed (.4f);
    //    AudioManager.enemyDetect.Play ();

    player.GetComponentInChildren<Animator> ().SetBool("isWalking", true);

    sq.Append (player.transform
      .DOLocalMove (gm.coordsToVec3(dest), .8f)
      .SetEase (Ease.OutQuad)
    );
    sq.OnKill (() => {
      currentCoords.Value = dest;

      player.GetComponentInChildren<Animator> ().SetBool("isWalking", false);


      var ec = node.enemyCount.Value;
      if (ec > 0) {
        Debug.Log ("enemy:" + ec);
        health.Value -= ec;

        foreach (var n in gm.graph.neighbors(dest)) {
          if (n == node) {
            continue;
          }
          //TODO:未探索nodeだとマイナスになる
          //0に補正するか0以下は表示しないか
          n.alertCount.Value = Mathf.Max(0, n.alertCount.Value - ec);
        }

        node.enemyCount.Value = 0;
        //          ec -= 1;

        //ランダム位置にワープ
        //TODO: 同じ位置に飛ばないようにする
        if (0 < ec) {
          //                      nodeList[Random.Range(0, nodeList.Count)].enemyCount.Value += ec;
        }

        AudioManager.maleScream.Play();
        while(ec-- > 0){
          createDead(dest);
        }

      }
      if(node.isExit){
        GameManager.Instance.onExit();
      }
      GameManager.Instance.alertCount.Value = node.alertCount.Value = gm.graph.scanEnemies (dest);
    });


  }
  void createDead(IntVector2 dest){
//    var diff = Random.insideUnitCircle * 2f;
    var range = 2f;
    var dead = Instantiate(
      survivorDeadPrefab, 
      gm.coordsToVec3(dest) + new Vector3(Random.Range(-range, range), 0.1f, Random.Range(-range, range)),
      Quaternion.Euler(new Vector3(20f, Random.Range(0,360f),0))
    ) as GameObject;

//    gm.addToViewContainer(dead);
    dead.transform.DOLocalRotate(
      new Vector3(90, dead.transform.rotation.eulerAngles.y, 0), Random.Range(.3f,.6f)
    ).SetEase(Ease.InCirc);
  }
}
