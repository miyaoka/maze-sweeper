using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
using DG.Tweening;

public class NodePresenter : MonoBehaviour {
	[SerializeField] CanvasGroup cg;
	[SerializeField] public Image floor;
	[SerializeField] Image wall;
	[SerializeField] CanvasGroup tile;
	[SerializeField] Text alertCountText;
	[SerializeField] Text enemyCountText;

//	public ReactiveProperty<int> enemyCount = new ReactiveProperty<int> (0);
//	public ReactiveProperty<int> alertCount = new ReactiveProperty<int> (0);
//	public ReactiveProperty<bool> visited = new ReactiveProperty<bool> ();
//	public ReactiveProperty<bool> onHere = new ReactiveProperty<bool> ();
//	public ReactiveProperty<IntVector2> coords = new ReactiveProperty<IntVector2>();
//	public ReactiveProperty<List<NodePresenter>> neighborNodes = new ReactiveProperty<List<NodePresenter>>( new List<NodePresenter>() );

//	List<CharacterPresenter> survivers = new List<CharacterPresenter> ();
//	List<CharacterPresenter> enemies = new List<CharacterPresenter> ();

//	CompositeDisposable envResources = new CompositeDisposable();


	CompositeDisposable modelResources = new CompositeDisposable();
	float fadeIn = .5f;
	float fadeOut = .2f;
	private NodeModel model;
	Sequence sq;
	Sequence wallSq;
	public NodeModel Model
	{
		set { 
			this.model = value; 
			sq = DOTween.Sequence ();

			modelResources.Clear ();

			model.alertCount
				.DistinctUntilChanged()
				.Subscribe (c => alertCountText.text = c == 0 ? "" : c.ToString())
				.AddTo (this);
			model.enemyCount
				.DistinctUntilChanged()
				.Subscribe (c => enemyCountText.text = c == 0 ? "" : c.ToString())
				.AddTo (this);

			var activeFloorColor = new Color (Random.Range (.7f, .9f), Random.Range (.7f, .9f),Random.Range (.7f, .9f));
			var visitedFloorColor = new Color (.2f, .2f, .2f);
			activeFloorColor = new Color(.8f,.8f,.8f);

			model.onHere
				.Subscribe (b => {
					var fade = b ? fadeIn : fadeOut;

					sq.Kill ();
					sq = DOTween.Sequence();
					if(b) {
//						sq.PrependInterval(.5f);
					}
					sq.Append(floor.DOColor(b ? activeFloorColor : visitedFloorColor, fade ).SetEase(Ease.OutQuad));
//					sq.Join(wall.DOColor(b ? new Color(.8f, .8f, .8f) : new Color(.2f, .2f, .2f), fade).SetEase(Ease.OutQuad));
//					sq.Join(wall.DOFade(b ? 1 : 0, fade).SetEase(Ease.OutQuad));
					sq.Join(tile.DOFade(b ? 1 : 0, fade).SetEase(Ease.OutQuad));
//									wall.gameObject.SetActive(b);
				})
				.AddTo (this);
			model.onDest
				.Subscribe (b => {
					if(b){
						wall.color = new Color(.2f, .2f, .2f, 0);
					}
					if(wallSq != null){
						wallSq.Kill();
					}
					wallSq = DOTween.Sequence();
					wallSq.Append(wall.DOFade(b ? 1 : 0, b ? .8f : 0).SetEase(Ease.OutQuad));
					wallSq.Append(wall.DOColor(b ? new Color(.8f, .8f, .8f, 1) : new Color(.2f, .2f, .2f, 0), b ? .5f : .2f).SetEase(Ease.OutQuad));
					return;
					wall.DOFade(b ? 1: 0, 1f).SetEase(Ease.OutQuad);
				})
				.AddTo (this);
			model.visited
				.Where (b => b)
				.DistinctUntilChanged()
				//			.Select (c => neighborBombCount())
				.Subscribe (c => {
					//				watchEnvs();
				})
				.AddTo (this);


			model.visited
				.DistinctUntilChanged()
				.Subscribe (b =>  {
					cg.DOFade(b ? 1 : 0, b ? 1 : 0).SetEase(Ease.OutQuad);
				})
				.AddTo (this);


		}
		get { return this.model; }
	}
	/*
	void Awake(){
		
	}
	void Start () {
		onHere = 
			GridManager.Instance
				.currentNode
				.Select(n => n == this)
				.DistinctUntilChanged ()
				.ToReactiveProperty ();
		alertCount
			.DistinctUntilChanged()
			.Subscribe (c => alertCountText.text = c == 0 ? "" : c.ToString())
			.AddTo (this);
		enemyCount
			.DistinctUntilChanged()
			.Subscribe (c => enemyCountText.text = c == 0 ? "" : c.ToString())
			.AddTo (this);


		var activeFloorColor = new Color (Random.Range (.7f, .9f), Random.Range (.7f, .9f),Random.Range (.7f, .9f));
		var visitedFloorColor = new Color (.2f, .2f, .2f);
		activeFloorColor = new Color(.8f,.8f,.8f);

		onHere
			.Subscribe (b => {
				floor.DOColor(b ? activeFloorColor : visitedFloorColor, .3f).SetEase(Ease.OutQuad);
				wall.DOFade(b ? 1 : 0, .3f).SetEase(Ease.OutQuad);
//				wall.gameObject.SetActive(b);
				tile.gameObject.SetActive(b);
			})
			.AddTo (this);

		visited
			.Where (b => b)
			.DistinctUntilChanged()
//			.Select (c => neighborBombCount())
			.Subscribe (c => {
//				watchEnvs();
			})
			.AddTo (this);
		

		visited
			.DistinctUntilChanged()
			.Subscribe (b =>  {
				view.SetActive (b);
			})
			.AddTo (this);


	}
	public int scanEnemies(){
		var ns = Neighbors;
		var count = 0;
		foreach (var n in ns) {
			count += n.enemyCount.Value;
		}
		alertCount.Value = count;
		return count;
	}
	void watchEnvs(){
		envResources.Clear ();

		var enemies = new List<ReactiveProperty<int>> {};
		foreach (var c in Neighbors) {
			enemies.Add (c.enemyCount);
		}

		Observable
			.CombineLatest (enemies.ToArray ())
			.Select (list => list.Sum())
			.Subscribe (v => alertCount.Value = v)
			.AddTo (envResources);
	}
	int neighborBombCount(){
		var ns = Neighbors;
		var count = 0;
		Debug.Log ("n:" + coords.Value.x + "," + coords.Value.y + " - " + ns.Count);
		foreach (var n in ns) {
			count += n.enemyCount.Value;
			Debug.Log (n.enemyCount.Value);
		}
		return count;
	}

	private List<NodePresenter> neighbors;
	public List<NodePresenter> Neighbors {
		get {
			if (neighbors == null) {
				neighbors = new List<NodePresenter> ();
				var cd = coords.Value;
				for (var ny = cd.y - 1; ny <= cd.y + 1; ny++) {
					for (var nx = cd.x - 1; nx <= cd.x + 1; nx++) {
						NodePresenter neignbor;
						try {
							neignbor = GridManager.Instance.nodes [nx, ny];
						} catch {
							continue;
						}
						neighbors.Add (neignbor);
					}
				}
			}
			return neighbors;
		}
	}

	void OnDestroy()
	{
		envResources.Dispose ();
	}
*/
}
