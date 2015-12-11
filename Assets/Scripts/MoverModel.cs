using UnityEngine;
using System.Collections;
using UniRx;
public class MoverModel {

	public ReactiveProperty<int> health = new ReactiveProperty<int>(5);
	public ReactiveProperty<bool> isAlive = new ReactiveProperty<bool>(true);
	public ReactiveProperty<IntVector2> coords;
	public MoverModel(IntVector2 coords){
		this.coords = new ReactiveProperty<IntVector2> (coords);

		this.coords
			.Buffer (2, 1)
			.Subscribe (l => {
				Debug.Log(l[0] + "," + l[1]);
			});
		
		isAlive = 
			health
				.Select (h => h > 0)
				.ToReactiveProperty ();
	}
}
