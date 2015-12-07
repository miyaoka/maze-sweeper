using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using DG.Tweening;

public enum Dirs { North, East, South, West};

public class GridManager : SingletonMonoBehaviour<GridManager> {
	[SerializeField] Transform gridNodeContainer;
	[SerializeField] Transform gridEdgeContainer;
	[SerializeField] int gridWidth = 3;
	[SerializeField] int gridHeight = 3;
	//1グリッドの大きさ
	[SerializeField] float gridUnit = 320;
	[SerializeField] GameObject gridNodePrefab;
	[SerializeField] GameObject gridEdgePrefab;
	[SerializeField] public Transform gridScale;
	[SerializeField] Transform gridPos;
	[SerializeField] bool showAll;

	public ReactiveProperty<IntVector2> currentCoords = new ReactiveProperty<IntVector2> ();
	public ReactiveProperty<NodeModel> currentNode = new ReactiveProperty<NodeModel> ();

	public List<NodeModel> nodeList = new List<NodeModel> ();
	public List<EdgeModel> edgeList = new List<EdgeModel> ();
	IntVector2[] dirCoords = {new IntVector2(0,1), new IntVector2(1,0), new IntVector2(0,-1), new IntVector2(-1,0)};

	GameManager gm;

	int divideMargin = 1;
	float passPerLong = 4f;


	void Awake ()
	{
		if (this != Instance) {
			Destroy (this);
			return;
		}
		DontDestroyOnLoad (this.gameObject);
	}
	void Start(){
		gm = GameManager.Instance;



		currentCoords
			.Subscribe (p => {
			}
			).AddTo (this);

	}
	public void scale(float s){
		gridScale.localScale = new Vector3(s, s, 1);
	}
	public void initGrid(){

		nodeList.Clear ();
		edgeList.Clear ();

		foreach (Transform t in gridEdgeContainer) {
			Destroy (t.gameObject);
		}
		foreach (Transform t in gridNodeContainer) {
			Destroy (t.gameObject);
		}

		//build nodes
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				//create model
				var model = new NodeModel (new IntVector2(x,y));
				model.enemyCount.Value = 0;

				//create go
				var pos = new Vector3 (x * gridUnit, y * gridUnit, 0);
				var go = Instantiate (gridNodePrefab, pos, Quaternion.identity) as GameObject;
				go.transform.SetParent (gridNodeContainer, false);

				//assign model to presenter
				go.GetComponent<NodePresenter> ().Model = model;

				//for debug		
				//				np.visited.Value = true;

//				nodes [x, y] = np;

				nodeList.Add (model);
			}
		}

		//create maze
		var rects = new List<Rect>();
		rects.Add(new Rect(new Vector2(0,0), new Vector2(gridWidth, gridHeight)));
		do {
			rects = createPass (rects);
		} while (rects.Count > 0);
			
		//行き止まりのnodeを削除する
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				var coords = new IntVector2 (x, y);
				var el = getAllEdgesFromNode (coords);
				if (el.Count <= 1) {
					if (Random.value < .5f) {
						removeNode (getNodeModel (coords));
						foreach(var e in el){
							removeEdge (e);
						}
					}
				}
			}
		}
		//create Enemies
		foreach (var n in nodeList) {
			n.enemyCount.Value = Random.value < .1f ? Random.Range(1,5) : 0;
		}

		//set init player position
		NodeModel initNode;
		do{
			initNode = getNodeModel(new IntVector2 (Random.Range(0,gridWidth), Random.Range(0,gridHeight)));
		} while (initNode == null || initNode.enemyCount.Value > 0 || initNode.scanEnemies() > 0);

		currentCoords.Value = initNode.coords;
		movePos (currentCoords.Value);

		//for debug
		if (showAll) {
			foreach (var n in nodeList) {
				n.visited.Value = true;
			}
		}


	}
	void createEdge(IntVector2 coords, Dirs dir){
		var pos = new Vector3 (coords.x * gridUnit, coords.y * gridUnit, 0);

		//create model
		var model = new EdgeModel(getNodeModel(coords), getNodeModel(coords + dirCoords[dir == 0 ? (int)Dirs.North : (int)Dirs.East]));

		//create go
		var go = Instantiate (gridEdgePrefab, pos, Quaternion.Euler(new Vector3(0,0, (int)dir *-90))) as GameObject;
		go.transform.SetParent (gridEdgeContainer, false);

		//assign model to presenter
		go.GetComponent<EdgePresenter>().Model = model;
		model.go = go;

		edgeList.Add (model);
	}
	void removeEdge(EdgeModel model){
		//destory presenter
		foreach (var p in gridEdgeContainer.GetComponentsInChildren<EdgePresenter> ()) {
			if (p.Model == model) {
				Destroy (p.gameObject);
			}
		}
		//remove model
		edgeList.Remove (model);
	}
	void removeNode(NodeModel model){
		//destory presenter
		foreach (var p in gridNodeContainer.GetComponentsInChildren<NodePresenter> ()) {
			if (p.Model == model) {
				Destroy (p.gameObject);
			}
		}
		//remove model
		nodeList.Remove (model);
	}

	public NodeModel getNodeModel(IntVector2 coords){
		return nodeList.Find (n => n.coords == coords);
	}
	public EdgeModel getEdgeModel(IntVector2 coords1, IntVector2 coords2){
		return edgeList.Find (e => e.CoordsList.Contains(coords1) && e.CoordsList.Contains(coords2));
	}
	public EdgeModel getEdgeModelByDir(IntVector2 coords, Dirs dir){
		return getEdgeModel (coords, coords + dirCoords[(int)dir]);
	}
	List<EdgeModel> getAllEdgesFromNode(IntVector2 coords){
		var list = new List<EdgeModel> ();
		foreach (var d in dirCoords)
		{
			var e = getEdgeModel (coords, coords + d);
			if (e == null) {
				continue;
			}
			list.Add(e);
		}
		return list;
	}


	public void moveDir(Dirs dir)
	{
		
		var edge = getEdgeModelByDir (currentCoords.Value, dir);
		if (edge == null) {
			return;
		}
//		if (!edge.type.Value.isPassable.Value) {
//			breachEdge(currentCoords.Value.x, currentCoords.Value.y, dir);
//			return;
//		}
		movePos (currentCoords.Value + dirCoords[(int)dir]);
	}
	void movePos(IntVector2 dest){
		var node = getNodeModel(dest);
		if (node == null) {
			return;
		}
		node.visited.Value = true;

//		Debug.Log ("move" + dest);

		//表示位置を変える
		gridPos
			.DOLocalMove (new Vector3 (-dest.x * gridUnit, -dest.y * gridUnit, 0), .2f)
			.SetEase (Ease.OutQuad)
			.OnComplete (() => {
				currentCoords.Value = dest;
				currentNode.Value = node;

				if (node.enemyCount.Value > 0) {
					Debug.Log ("bomb:" + node.enemyCount.Value);
					var ec = node.enemyCount.Value;

					foreach (var n in node.Neighbors) {
						if (n == node) {
							continue;
						}
						//TODO:未探索nodeだとマイナスになる
						//0に補正するか0以下は表示しないか
						n.alertCount.Value = Mathf.Max(0, n.alertCount.Value - ec);
					}
					node.enemyCount.Value = 0;
					//			ec -= 1;

					//ランダム位置にワープ
					//TODO: 同じ位置に飛ばないようにする
					if (0 < ec) {
						nodeList[Random.Range(0, nodeList.Count)].enemyCount.Value += ec;
					}

				} 
				gm.alertCount.Value = node.scanEnemies ();
			});


	}

	public List<Rect> createPass(List<Rect> rects){
		Rect rect, divRect1, divRect2;
		float area;

		//pickup dividable rect
		do {
			if (rects.Count == 0) {
				return rects;
			}
			rect = rects [0];
			rects.RemoveAt (0);

			area = rect.width * rect.height;
		} while(area < 2);

		var isHorizontalRect = (int)rect.width == (int)rect.height ? Random.Range(0,2) == 0 : rect.width > rect.height;
		var longSide = Mathf.Max ((int)rect.width, (int)rect.height);
		var shortSide = Mathf.Min ((int)rect.width, (int)rect.height);

		//min divide span
		divideMargin = Mathf.Min( Mathf.FloorToInt(longSide *.5f) - 1, divideMargin);
		var divPt = Random.Range (divideMargin, longSide - 1 - divideMargin);

		//list passable points
		var passPts = new List<int>();
		for (var i = 0; i < shortSide; i++) {
			passPts.Add (i);
		}
		var pCount = (float)shortSide / passPerLong;
		//create passage at random points
		while (pCount-- > 0) {
			var passIndex = Random.Range (0, passPts.Count);
			var passPt = passPts [passIndex];
			passPts.RemoveAt (passIndex);
			var startCoords = new IntVector2 ((int)rect.xMin, (int)rect.yMin);
			var passCoords = isHorizontalRect 
			? new IntVector2 (divPt, passPt)
			: new IntVector2 (passPt, divPt);
			createEdge (startCoords + passCoords, isHorizontalRect ? Dirs.East : Dirs.North);
		}
		//divide more if has enough area
		if (area > 2) {
			divRect1 = divRect2 = rect;			

			divPt++;
			if (isHorizontalRect) {
				divRect1.width = divPt;
				divRect2.xMin += divPt;
			} else {
				divRect1.height = divPt;
				divRect2.yMin += divPt;
			}
			rects.Insert (0, divRect1);
			rects.Insert (0, divRect2);
		}

		return rects;
	}
	void changeEdgeType(IntVector2 coords, Dirs dir, EdgeType edgetype)
	{
		Debug.Log (edgetype.isPassable);
		var model = getEdgeModelByDir (coords, dir);
		if (model == null) {
			return;
		}
		model.type.Value = edgetype;
	}
}
