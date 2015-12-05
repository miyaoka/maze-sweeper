using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;

public class NodePresenter : MonoBehaviour {
	[SerializeField] GameObject view;
	[SerializeField] Image floor;
	[SerializeField] Image wall;
	[SerializeField] GameObject tile;
	[SerializeField] Text alertCountText;
	[SerializeField] Text enemyCountText;

	public ReactiveProperty<int> enemyCount = new ReactiveProperty<int> ();
	public ReactiveProperty<int> alertCount = new ReactiveProperty<int> ();
	public ReactiveProperty<bool> visited = new ReactiveProperty<bool> ();
	public ReactiveProperty<bool> onHere = new ReactiveProperty<bool> ();
	public ReactiveProperty<IntVector2> coords = new ReactiveProperty<IntVector2>();
//	public ReactiveProperty<List<NodePresenter>> neighborNodes = new ReactiveProperty<List<NodePresenter>>( new List<NodePresenter>() );


	CompositeDisposable envResources = new CompositeDisposable();

	void Awake(){
		
	}
	void Start () {
		var gm = GameManager.Instance;
		onHere = 
			gm.currentCoords
				.CombineLatest(coords, (l,r) => l == r)
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

		/*
		neighborCells
			.Subscribe (elist => watchEnvs())
			.AddTo (this);

							*/

		var floorColor = new Color (Random.Range (.7f, .9f), Random.Range (.7f, .9f),Random.Range (.7f, .9f));
		floorColor = new Color(.8f,.8f,.8f);

		onHere
			.Subscribe (b => {
				floor.color = b ? floorColor : new Color(.2f,.2f,.2f);
				wall.gameObject.SetActive(b);
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
							neignbor = GameManager.Instance.nodes [nx, ny];
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
}
