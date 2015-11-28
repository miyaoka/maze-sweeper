using UnityEngine;
using System.Collections;
using UniRx;
public class CellPresenter : MonoBehaviour {
	const int E = 1 << 0;
	const int N = 1 << 1;
	const int W = 1 << 2;
	const int S = 1 << 3;
	[SerializeField] GameObject WallE;
	[SerializeField] GameObject WallN;
	[SerializeField] GameObject WallW;
	[SerializeField] GameObject WallS;
	[SerializeField] TextMesh BombNumText;

	public ReactiveProperty<int> wallBit = new ReactiveProperty<int> ();
	public ReactiveProperty<int> envBomb = new ReactiveProperty<int> ();
	public ReactiveProperty<int> bomb = new ReactiveProperty<int> ();

	void Start () {
		wallBit
			.Subscribe (w => {
				WallE.SetActive( (w & E) != 0);
				WallN.SetActive( (w & N) != 0);
				WallW.SetActive( (w & W) != 0);
				WallS.SetActive( (w & S) != 0);
			})
			.AddTo (this);
		envBomb.
		Subscribe (b => {
			BombNumText.text = (b == 0) ? "" : b.ToString();
		})
			.AddTo (this);

		bomb
			.Subscribe (b => {
		})
			.AddTo (this);

	}
}
