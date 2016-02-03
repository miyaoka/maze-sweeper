using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using System.Linq;
using UnityEngine.Events;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
  [SerializeField]
  GameObject survivorPrefab;
  [SerializeField]
  GameObject bloodPrefab;
  [SerializeField]
  GameObject alienPrefab;

  GameObject player;
  GameObject playerBody;
  public ReactiveProperty<IntVector2> CurrentCoords = new ReactiveProperty<IntVector2>();
  public ReactiveProperty<IntVector2> DestCoords = new ReactiveProperty<IntVector2>();

  Sequence sq;
  GraphManager graph;
  RoundManager gm;

  void Awake()
  {
    if (this != Instance)
    {
      Destroy(this);
      return;
    }
    graph = GraphManager.Instance;
    gm = RoundManager.Instance;


    /*
    player
      .GetComponentsInChildren<Collider>()
      .ToList()
      .ForEach(c => Destroy(c));
      */

  }
  void Start()
  {
  }
  public void InitPlayer(IntVector2 dest)
  {
    player = Instantiate(survivorPrefab);
    graph.AddToView(player);
    playerBody = player.GetComponent<SurvivorPresenter>().body;
    MovePos(dest, true);
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

    playerBody.transform.DORotate(new Vector3(0, (int)dir * -90 + 90, 0), .5f).SetEase(Ease.InOutQuad);

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
    var node = graph.graph.GetNode(CurrentCoords.Value);
    var edge = node.EdgeArray[(int)dir];
    if (edge == null)
    {
      return;
    }

    edge.isOpened.Value = true;

    MovePos(CurrentCoords.Value + GraphModel.DirCoords[(int)dir]);
  }
  public void MovePos(IntVector2 dest, bool noAnim = false)
  {
    RoundManager.Instance.OnBomb.Value = false;
    var node = graph.ShowNode(dest);
    if (node == null)
    {
      return;
    }
    DestCoords.Value = dest;
    graph.ScanEnemies(node);

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

    if(node.EnemyCount.Value > 0)
    {
      gm.ViewState.Value = ViewStateName.Battle;
    }
    if (noAnim)
    {
      player.transform.localPosition = graph.CoordsToVec3(dest);
      onMoved(node);
      return;
    }

    CameraManager.Instance.MovePos(dest);

    //if already moving, force complete.
    if (sq != null)
    {
      sq.Kill();
    }
    sq = DOTween.Sequence();
    startWalk();
    sq.Append(player.transform
      .DOLocalMove(graph.CoordsToVec3(dest), 1f)
      .SetEase(Ease.InOutQuad)
      .OnComplete(stopWalk)
    );
    sq.OnKill(() =>
    {
      onMoved(node);
    });
  }
  public void ShowDamage(int damage)
  {
    player.GetComponent<SurvivorPresenter>().ShowDamage(damage);
  }
  private void onMoved(Node node)
  {
    if (RoundManager.Instance.IsAllDead.Value)
    {
      return;
    }
    CurrentCoords.Value = node.Coords;
    var ec = node.EnemyCount.Value;
    if (ec > 0)
    {
      StartCoroutine( enemyAttacks(ec, ()=>
      {
        graph.ClearNodeEnemy(node);
        graph.ScanEnemies(node);

        ShowDamage(ec);

        if (SurvivorManager.Instance.LivingList.Count > 0)
        {
          stopKnock();
          gm.ViewState.Value = ViewStateName.Move;
        }
      }));

    }
    if (graph.isExit(node))
    {
      RoundManager.Instance.onExit();
    }
    if (node.HasItem.Value)
    {
      node.HasItem.Value = false;
      RoundManager.Instance.AddTime();
      player.GetComponent<SurvivorPresenter>().ShowMsg("+30sec");
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

  public void CreateDead(Survivor sv)
  {
    AudioManager.Scream.Play();
    //last one guy
    if (SurvivorManager.Instance.LivingList.Count == 0)
    {
      player.GetComponentInChildren<Animator>().enabled = false;
      player.GetComponent<SurvivorPresenter>().nameText.text = sv.Name.Value;
      gm.AllDead();
      return;
    }
    var range = 3f;
    var dead = Instantiate(player);
    dead.transform.position = 
    graph.CoordsToVec3(CurrentCoords.Value) + new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));

    var sp = dead.GetComponent<SurvivorPresenter>();
    sp.nameText.text = sv.Name.Value;
    sp.body.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360f), 0));

    dead.GetComponentInChildren<Animator>().enabled = false;
    graph.AddToView(dead);
  }
  IEnumerator enemyAttacks(int count, UnityAction onComplete)
  {
    //    var diff = Random.insideUnitCircle * 2f;
    var radUnit = Mathf.PI * 2 / count;
    var baseRad = Random.Range(0, Mathf.PI * 2);
    var basePos = graph.CoordsToVec3(CurrentCoords.Value);
    var complete = 0;

    for (var i = 0; i < count; i++)
    {
      var rad = Random.Range(i, i + .8f) * radUnit + baseRad;
      var len = Random.Range(2f, 4f);

      enemyAttack(
        basePos + new Vector3(Mathf.Sin(rad) * len, 0f, Mathf.Cos(rad) * len),
        basePos,
        () => { complete += 1; },
        i == 0 ? 0 : Random.Range(0, count * .2f)
        );
    }
    while (complete < count)
    {
      yield return null;
    }
    onComplete();
  }
  void enemyAttack(Vector3 initPos, Vector3 targetPos, UnityAction onComplete, float delay)
  {
    var enemy = Instantiate(
      alienPrefab,
      initPos,
      Quaternion.identity
    ) as GameObject;

    enemy.transform.LookAt(player.transform);
    enemy.transform.position += new Vector3(0, -2, 0);
    graph.AddToView(enemy);

    var attack = .2f;
    var seq = DOTween.Sequence();

    //popup
    seq.Append(
      enemy.transform
        .DOMoveY(0, .2f)
        .SetEase(Ease.OutQuad)
      );

    //attack
    seq.Append(
      enemy.transform
      .DOMoveY(Random.Range(1f, 2f), attack)
      .SetEase(Ease.OutBack)
      .SetDelay(delay)
      );
    seq.Join(
      enemy.transform
      .DOMoveX(targetPos.x, attack)
      .SetEase(Ease.OutSine)
      );
    seq.Join(
      enemy.transform
      .DOMoveZ(targetPos.z, attack)
      .SetEase(Ease.OutSine)
      );
    seq.AppendCallback(() =>
    {
      startKnock();
      AudioManager.Damage.Play();
      CameraManager.Instance.shake();
      SurvivorManager.Instance.AddDamages(1);
    });

    //return and hide
    seq.Append(
       enemy.transform
            .DOMove(initPos, .3f)
            .SetEase(Ease.OutSine)
      );
    seq.AppendCallback(() =>
    {
      onComplete();
      Destroy(enemy);
    });

//    yield return null;
  }
  void startKnock()
  {
    player.GetComponentInChildren<Animator>().SetBool("IsKnocked", true);
  }
  void stopKnock()
  {
    player.GetComponentInChildren<Animator>().SetBool("IsKnocked", false);
  }
}
