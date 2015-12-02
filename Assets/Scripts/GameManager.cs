using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public class GameManager : SingletonMonoBehaviour<GameManager>{
	[SerializeField] GameObject gridNodePrefab;
	[SerializeField] GameObject gridEdgePrefab;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] Transform gridNodeContainer;
	[SerializeField] Transform gridEdgeContainer;
	[SerializeField] Transform gridScale;
	[SerializeField] Transform gridPos;
	[SerializeField] Slider slider;

	[SerializeField] int gridWidth = 3;
	[SerializeField] int gridHeight = 3;
	//1グリッドの大きさ
	[SerializeField] float gridUnit = 320;

	public ReactiveProperty<IntVector2> currentCoords = new ReactiveProperty<IntVector2> ();
	public NodePresenter[,] nodes;
	public EdgePresenter[,,] edges;
	List<Rect> rects = new List<Rect> ();

	enum Dirs { T, R, B, L};
	IntVector2[] dirVectors = {new IntVector2(0,1), new IntVector2(1,0), new IntVector2(0,-1), new IntVector2(-1,0)};

	void Awake ()
	{
		if (this != Instance) {
			Destroy (this);
			return;
		}
		DontDestroyOnLoad (this.gameObject);
	}
	void Start(){
		nodes = new NodePresenter[gridWidth, gridHeight];
		edges = new EdgePresenter[gridWidth, gridHeight, 2];
		initGrid ();

		var update = this
			.UpdateAsObservable ();

		//zoom control
		update
			.Select (_ => slider.value)
			.Subscribe (v => {
				gridScale.localScale = new Vector3(v, v, 1);
			})
			.AddTo (this);

		currentCoords
			.Subscribe (p => {
		}
		).AddTo (this);

		//move control
		update
			.Where (up => Input.GetKeyDown (KeyCode.W))
			.Subscribe (_ => moveDir (Dirs.T))
			.AddTo (this);
		update
			.Where (down => Input.GetKeyDown (KeyCode.S))
			.Subscribe (_ => moveDir (Dirs.B))
			.AddTo (this);
		update
			.Where (right => Input.GetKeyDown (KeyCode.D))
			.Subscribe (_ => moveDir (Dirs.R))
			.AddTo (this);
		update
			.Where (left => Input.GetKeyDown (KeyCode.A))
			.Subscribe (_ => moveDir (Dirs.L))
			.AddTo (this);

		update
			.Where (up => Input.GetKeyDown (KeyCode.Space))
			.Subscribe (_ => {
				//div(rects);
				while(div(rects)){}
			})
			.AddTo (this);
	}
	void moveDir(Dirs dir)
	{
		var edge = getEdge (currentCoords.Value.x, currentCoords.Value.y, dir);
		if (!edge.isPassable.Value) {
			breachEdge(currentCoords.Value.x, currentCoords.Value.y, dir);
			return;
		}
		movePos (currentCoords.Value + dirVectors[(int)dir]);
	}
	void movePos(IntVector2 dest){
		NodePresenter node;
		try{
			node = nodes [dest.x, dest.y];
		}
		catch{
			return;
		}
		currentCoords.Value = dest;
		node.visited.Value = true;

		//表示位置を変える
		gridPos.localPosition = new Vector3 (-currentCoords.Value.x * gridUnit, -currentCoords.Value.y * gridUnit, 0);

		if (node.bombCount.Value > 0) {
			Debug.Log ("bomb:" + node.bombCount.Value);
			node.bombCount.Value--;
		}
	}
	void initGrid(){
		foreach (Transform t in gridEdgeContainer) {
			Destroy (t.gameObject);
		}
		foreach (Transform t in gridNodeContainer) {
			Destroy (t.gameObject);
		}

		//build nodes
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				var pos = new Vector3 (x * gridUnit, y * gridUnit, 0);
				var node = Instantiate (gridNodePrefab, pos, Quaternion.identity) as GameObject;
				node.transform.SetParent (gridNodeContainer, false);
				var np = node.GetComponent<NodePresenter> ();
				np.bombCount.Value = Random.value < .1f ? Random.Range(1,5) : 0;
//				np.visited.Value = true;
				np.coords.Value = new IntVector2 (x, y);
				nodes [x, y] = np;
			}
		}
		//build edges
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				var pos = new Vector3 (x * gridUnit, y * gridUnit, 0);
				//d0 = top, d1 = right
				for (var d = 0; d < 2; d++) {
					var edge = Instantiate (gridEdgePrefab, pos, Quaternion.Euler(new Vector3(0,0, d *-90))) as GameObject;
					edge.transform.SetParent (gridEdgeContainer, false);
					var ep = edge.GetComponent<EdgePresenter> ();
					ep.nodeFrom.Value = nodes [x, y];
					if (d == 0){
						if (y + 1 == gridHeight) {
							ep.type.Value = EdgeType.wall;
						} else {
							ep.nodeTo.Value = nodes [x, y + 1];
						}
					}
					else {
						if (x + 1 == gridWidth) {
							ep.type.Value = EdgeType.wall;
						} else {
							ep.nodeTo.Value = nodes [x + 1, y];
						}
					}
					edges [x, y, d] = ep;
				}

			}
		}
		currentCoords.Value = new IntVector2 ((int)(gridWidth * .5f), (int)(gridHeight * .5f));
		movePos (IntVector2.Zero);

		rects.Clear();
		rects.Add(new Rect(new Vector2(0,0), new Vector2(gridWidth, gridHeight)));
		while (div (rects)) {}


		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				nodes [x, y].visited.Value = true;
			}
		}
		/*
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				var c = edgeCount (x, y);
				if (c <= 1) {
//					nodes [x, y].hide ();
					for (var d = 0; d < 2; d++) {
//						edges [x, y, d].type.Value = EdgeType.wall;
					}
				}
			}
		}
		*/
			
	}
	EdgeType getEdge(int x, int y, Dirs dir){
		EdgeType et = EdgeType.wall;
		try{
			switch (dir) {
			case Dirs.T:
				et = edges [x, y, 0].type.Value;
				break;
			case Dirs.R:
				et = edges [x, y, 1].type.Value;
				break;
			case Dirs.B:
				et = edges [x, y-1, 0].type.Value;
				break;
			case Dirs.L:
				et = edges [x-1, y, 1].type.Value;
				break;
			}
		}
		catch{
		}
		return et;
	}
	void breachEdge(int x, int y, Dirs dir){
		try{
			switch (dir) {
			case Dirs.T:
				edges [x, y, 0].breach();
				break;
			case Dirs.R:
				edges [x, y, 1].breach();
				break;
			case Dirs.B:
				edges [x, y-1, 0].breach();
				break;
			case Dirs.L:
				edges [x-1, y, 1].breach();
				break;
			}
		}
		catch{
		}		
	}
	int edgeCount(int x, int y){
		int count = 0;
		for (var i = 0; i < 4; i++) {
			count += getEdge (x, y, (Dirs)i).isPassable.Value ? 1 : 0;
		}
		return count;
	}
	bool div(List<Rect> rects){
		Rect rect;
		while (true) {
			if (rects.Count == 0) {
				return false;
			}

			rect = rects [0];
			rects.RemoveAt (0);

//			var minSize = Random.Range(1,2);
			if (rect.width <=  Random.Range(1,3) && rect.height <=  Random.Range(1,3)) {
				/*
				if (Random.value < .3f) {
					for (int y = (int)rect.yMin; y < (int)rect.yMax; y++) {
						for (int x = (int)rect.xMin; x < (int)rect.xMax; x++) {
							grid [y] [x] |= ROOM;
						}
					}

				}
				*/
				//			return;
			} else {
				break;
			}
		}

		var isHorizontal = (int)rect.width == (int)rect.height ? Random.Range(0,2) == 0 : rect.width < rect.height;
		var rect1 = rect;
		var rect2 = rect;
		//壁を作り、その中に通過地点を作る
		if (isHorizontal) {
			int wallY = (int)Mathf.Floor(Random.Range(rect.yMin, rect.yMax-2));
			for (int x = (int)rect.xMin; x < rect.xMax; x++) {
				edges[x, wallY, EdgeType.DIR_TOP].type.Value = EdgeType.wall;
			}
			var px = (int)Random.Range (rect.xMin, rect.xMax);
			edges [px, wallY, EdgeType.DIR_TOP].type.Value = EdgeType.passage;
			if (rect.width > 5) {
				px = (px +(int)Random.Range (2, rect.width - 1)) % (int)rect.width;
				edges [px, wallY, EdgeType.DIR_TOP].type.Value = EdgeType.passage;
			}

			rect1.yMax = wallY + 1;
			rect2.yMin = wallY + 1;
		} else {
			int wallX = (int)Mathf.Floor(Random.Range(rect.xMin, rect.xMax-2));
			for (int y = (int)rect.yMin; y < rect.yMax; y++) {
				edges [wallX, y, EdgeType.DIR_RIGHT].type.Value = EdgeType.wall;
			}
			var py = (int)Random.Range (rect.yMin, rect.yMax);
			edges [wallX, py, EdgeType.DIR_RIGHT].type.Value = EdgeType.passage;
			if (rect.height > 5) {
				py = (py +(int)Random.Range (2, rect.height - 1)) % (int)rect.height;
				edges [wallX, py, EdgeType.DIR_RIGHT].type.Value = EdgeType.passage;
			}
			rect1.xMax = wallX + 1;
			rect2.xMin = wallX + 1;
		}
		rects.Insert (0, rect1);
		rects.Insert (0, rect2);
		return true;
	}

}