using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
public class CellPresenter : MonoBehaviour {
	public const int N = 1 << 0;
	public const int E = 1 << 1;
	public const int S = 1 << 2;
	public const int W = 1 << 3;
	public const int ALLDIR = N | E | S | W;
	[SerializeField] GameObject WallE;
	[SerializeField] GameObject WallN;
	[SerializeField] GameObject WallW;
	[SerializeField] GameObject WallS;
	[SerializeField] GameObject bombObject;
	[SerializeField] GameObject fade;
	[SerializeField] TextMesh BombNumText;

	ReactiveProperty<int> neighbourBombsCount = new ReactiveProperty<int> ();
	public ReactiveProperty<int> wallBit = new ReactiveProperty<int> ();
	public ReactiveProperty<int> bomb = new ReactiveProperty<int> ();
	public ReactiveProperty<List<CellPresenter>> neighborCells = new ReactiveProperty<List<CellPresenter>>( new List<CellPresenter>() );
	public ReactiveProperty<int> visited = new ReactiveProperty<int> ();

	CompositeDisposable envResources = new CompositeDisposable();

	void Start () {
		wallBit
			.Subscribe (w => {
				WallE.SetActive( (w & E) != 0);
				WallN.SetActive( (w & N) != 0);
				WallW.SetActive( (w & W) != 0);
				WallS.SetActive( (w & S) != 0);
			})
			.AddTo (this);
		neighbourBombsCount.
		Subscribe (b => {
			BombNumText.text = (b == 0) ? "" : b.ToString();
		})
			.AddTo (this);

		bomb
			.Subscribe (b => bombObject.SetActive(b > 0))
			.AddTo (this);

		neighborCells
			.Subscribe (elist => watchEnvs())
			.AddTo (this);

		visited
			.Select (v => v > 0)
			.Subscribe (v => fade.SetActive (v))
			.AddTo (this);


	}
	void watchEnvs(){
		envResources.Clear ();

		var bombs = new List<ReactiveProperty<int>> {};
		foreach (var c in neighborCells.Value) {
			bombs.Add (c.bomb);
		}

		Observable
			.CombineLatest (bombs.ToArray ())
			.Select (list => list.Sum())
			.Subscribe (v => neighbourBombsCount.Value = v)
			.AddTo (envResources);
	}
}
