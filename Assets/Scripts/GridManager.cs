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
	[SerializeField] public float gridUnit = 320;
	[SerializeField] GameObject gridNodePrefab;
	[SerializeField] GameObject gridEdgePrefab;
	[SerializeField] bool showAll;


	public List<NodeModel> nodeList = new List<NodeModel> ();
	public List<EdgeModel> edgeList = new List<EdgeModel> ();
	public List<MoverModel> enemyList = new List<MoverModel> ();
	public static readonly IntVector2[] dirCoords = {new IntVector2(0,1), new IntVector2(1,0), new IntVector2(0,-1), new IntVector2(-1,0)};

	GameManager gm;

	[SerializeField] int divideMargin = 1;
	[SerializeField] float passPerLong = 4f;
	[SerializeField] float deadEndReduceProb = .5f;
	[SerializeField] float enemyDeployProb = .1f;
	[SerializeField] int maxEnemyCount = 5;


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
	}
	public void scale(float s){
//		gridScale.localScale = new Vector3(s, s, 1);
	}
	public void initGrid(){
		nodeList.Clear ();
		edgeList.Clear ();
		clearView ();

		//models
		//
		//create nodes
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				createNode(new IntVector2(x,y));
			}
		}
		//create edges (maze)
		var rects = new List<Rect>();
		rects.Add(new Rect(new Vector2(0,0), new Vector2(gridWidth, gridHeight)));
		do {
			rects = divideRect (rects);
		} while (rects.Count > 0);
			
		//remove dead-end node
		for (var i = nodeList.Count - 1; i >= 0; i--) {
			var n = nodeList [i];
			var el = getAllEdgesFromNode (n.coords);
			//remove non-connected node
			if (el.Count == 0) {
				nodeList.RemoveAt (i);
				continue;
			}
			//deadend node
			if(el.Count == 1 && Random.value < deadEndReduceProb) {
				edgeList.Remove (el [0]);
				nodeList.RemoveAt (i);
			}
		}

		//build view by models
		createView();

		//create Enemies
		foreach (var n in nodeList) {
			var enemyCount = Random.value < enemyDeployProb ? Random.Range(1,maxEnemyCount) : 0;
			n.enemyCount.Value = enemyCount;
			while (enemyCount-- > 0) {
				var e = new MoverModel (n.coords);
				enemyList.Add (e);
			}
		}

		//set init player position
		NodeModel initNode;
		do{
			initNode = getNodeModel(new IntVector2 (Random.Range(0,gridWidth), Random.Range(0,gridHeight)));
		} while (initNode == null || initNode.enemyCount.Value > 0 || initNode.scanEnemies() > 0);

		PlayerManager.Instance.currentCoords.Value = initNode.coords;
//		movePos (currentCoords.Value);

		//for debug
		if (showAll) {
			foreach (var n in nodeList) {
				n.visited.Value = true;
			}
		}


	}
	public void moveEnemy(MoverModel e, IntVector2 c1, IntVector2 c2){
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



	/*
	 * create maze by adding passage edges to rects
	 */ 
	public List<Rect> divideRect(List<Rect> rects){
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
		var margin = Mathf.Min( Mathf.FloorToInt(longSide *.5f) - 1, divideMargin);
		var divPt = Random.Range (margin, longSide - 1 - margin);

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


	void createNode(IntVector2 coords){
		var model = new NodeModel (coords);
//		model.enemyList.Value.Add(new EnemyModel());
//		model.enemyList.Value.Add(new EnemyModel());
//		model.enemyList2.Add(new EnemyModel());
		nodeList.Add (model);

//		Debug.Log (model.getHealth());
	}
	void createEdge(IntVector2 coords, Dirs dir){
		var model = new EdgeModel(getNodeModel(coords), getNodeModel(coords + dirCoords[dir == 0 ? (int)Dirs.North : (int)Dirs.East]), dir);
		edgeList.Add (model);
	}
	void createView(){
		foreach (var node in nodeList) {
			var go = Instantiate (gridNodePrefab, coordsToVec3(node.coords), Quaternion.identity) as GameObject;
			go.transform.SetParent (gridNodeContainer, false);
			go.GetComponent<NodePresenter> ().Model = node;
		}
		foreach (var edge in edgeList) {
			var go = Instantiate (gridEdgePrefab, coordsToVec3(edge.nodes[0].coords), Quaternion.Euler(new Vector3(0,0, (int)edge.dir *-90))) as GameObject;
			go.transform.SetParent (gridEdgeContainer, false);
			go.GetComponent<EdgePresenter>().Model = edge;
		}
	}
	void clearView(){
		foreach (Transform t in gridEdgeContainer) {
			Destroy (t.gameObject);
		}
		foreach (Transform t in gridNodeContainer) {
			Destroy (t.gameObject);
		}
	}
	public Vector3 coordsToVec3(IntVector2 coords){
		return new Vector3 (coords.x * gridUnit, coords.y * gridUnit, 0);
	}
	void removeEdgeWithView(EdgeModel model){
		//destory presenter
		foreach (var p in gridEdgeContainer.GetComponentsInChildren<EdgePresenter> ()) {
			if (p.Model == model) {
				Destroy (p.gameObject);
			}
		}
		//remove model
		edgeList.Remove (model);
	}
	void removeNodeWithView(NodeModel model){
		//destory presenter
		foreach (var p in gridNodeContainer.GetComponentsInChildren<NodePresenter> ()) {
			if (p.Model == model) {
				Destroy (p.gameObject);
			}
		}
		//remove model
		nodeList.Remove (model);
	}

}
