using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using System.Collections.Generic;
using System.Linq;
public class NodeModel {

	public IntVector2 coords;

	public ReactiveProperty<int> enemyCount = new ReactiveProperty<int> (0);
	public ReactiveProperty<int> alertCount = new ReactiveProperty<int> (0);
	public ReactiveProperty<bool> visited = new ReactiveProperty<bool> ();
	public ReactiveProperty<bool> onHere = new ReactiveProperty<bool> ();
	public ReactiveProperty<bool> onDest = new ReactiveProperty<bool> ();
/*
	public List<MoverModel> enemyList = new List<MoverModel>();
	public List<MoverModel> EnemyList{
		get { return new List<MoverModel>(enemyList); }
	}
*/
	CompositeDisposable enemyResources = new CompositeDisposable();
	public NodeModel(IntVector2 coords){
		this.coords = coords;


		onHere = 
			PlayerManager.Instance
				.currentCoords
				.Select(n => n == coords)
				.DistinctUntilChanged ()
				.ToReactiveProperty ();
		onDest = 
			PlayerManager.Instance
				.destCoords
				.Select(n => n == coords)
				.DistinctUntilChanged ()
				.ToReactiveProperty ();

		onHere
			.Where(h => h)
			.Subscribe (o => {
		});
	}


	public int scanEnemies(){
		var ns = Neighbors;
		var count = 0;
		foreach (var n in ns) {
			count += n.enemyCount.Value;
		}
//		alertCount.Value = count;
		return count;
	}
	/*
	public void addEnemies(List<MoverModel> enemies){
		enemyList.AddRange (enemies);
		watchEnemies ();
	}
	public void moveEnemies(NodeModel dest){
		var list = new List<MoverModel> ();
		for (var i = enemyList.Count - 1; i >= 0; i--) {
			var e = enemyList [i];
			if (e.isAlive.Value) {
				enemyList.RemoveAt (i);
				list.Add (e);
			}
		}
		dest.addEnemies (list);
		watchEnemies ();
	}
	void watchEnemies(){
		enemyResources.Clear ();
		Observable
			.CombineLatest (enemyList.Select(e => e.isAlive).ToArray())
			.Select (l => l.Sum(alive => alive ? 1 : 0))
			.Do (i => Debug.Log (enemyCount + ":" + i))
			.Subscribe (i => enemyCount.Value = i)
			.AddTo (enemyResources);
	}
	*/

	private List<NodeModel> neighbors;
	public List<NodeModel> Neighbors {
		get {
			if (neighbors == null) {
				neighbors = new List<NodeModel> ();
				for (var nx = coords.x - 1; nx <= coords.x + 1; nx++) {
					for (var ny = coords.y - 1; ny <= coords.y + 1; ny++) {
						var dest = new IntVector2 (nx, ny);
						if (dest == coords) {
							continue;
						}
						var neighbor = GridManager.Instance.getNodeModel(dest);
						if (neighbor != null) {
							neighbors.Add (neighbor);
						}
					}
				}
			}
			return neighbors;
		}
	}

}
