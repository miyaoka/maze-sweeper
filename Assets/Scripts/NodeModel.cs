using UnityEngine;
using System.Collections;
using UniRx;
using System.Collections.Generic;
public class NodeModel {

	public IntVector2 coords;

	public ReactiveProperty<int> enemyCount = new ReactiveProperty<int> (0);
	public ReactiveProperty<int> alertCount = new ReactiveProperty<int> (0);
	public ReactiveProperty<bool> visited = new ReactiveProperty<bool> ();
	public ReactiveProperty<bool> onHere = new ReactiveProperty<bool> ();


	public NodeModel(IntVector2 coords){
		this.coords = coords;

		onHere = 
			GridManager.Instance
				.currentCoords
				.Select(n => n == coords)
				.DistinctUntilChanged ()
				.ToReactiveProperty ();
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
