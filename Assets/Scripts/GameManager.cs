using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public class GameManager : MonoBehaviour {

	[SerializeField] GameObject floorPrefab;
	[SerializeField] GameObject floorDivPrefab;
	[SerializeField] GameObject wallPrefab;
	[SerializeField] GameObject blockPrefab;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject cellPrefab;
	[SerializeField] Transform gridContainer;
	[SerializeField] Transform camera;
	[SerializeField] Slider slider;
	[SerializeField] RectTransform gridContainer2;

	GameObject player;
	int gridWidth = 20;
	int gridHeight = 10;
	int playerX;
	int playerY;
	float cellScale = 1;
	float cellSpan = .2f;
	List<List<CellPresenter>> grid = new List<List<CellPresenter>> ();
	List<Rect> rects = new List<Rect> ();

	// Use this for initialization
	void Start () {
		player = Instantiate (playerPrefab, new Vector3 (0, .5f, 0), Quaternion.identity) as GameObject;

		//init grid
		createGrid();
		initGrid ();
					



		var update = this
			.UpdateAsObservable ();

		update
			.Select (_ => slider.value)
			.Subscribe (v => {
				gridContainer2.localScale = new Vector3(v, v, 1);
		})
			.AddTo (this);

		update
			.Where (up => Input.GetKeyDown (KeyCode.Backspace))
			.Subscribe (_ => {
				initGrid();
			})
			.AddTo (this);
		
		update
			.Select (_ => player.transform.position)
			.Subscribe (pos => camera.position = new Vector3(pos.x, camera.position.y, pos.z))
			.AddTo (this);

//		displayMaze (grid);

		update
			.Where (up => Input.GetKeyDown (KeyCode.Space))
			.Subscribe (_ => {

//				div(rects);
				while(div(rects)){}
//				displayMaze (grid);
			})
			.AddTo (this);
		update
			.Where (up => Input.GetKey (KeyCode.R))
			.Subscribe (_ => {

//				addRoom(new Rect(Random.Range(0, gridWidth-10),Random.Range(0,gridHeight-10),Random.Range(3,10),Random.Range(3,10)));
//				displayMaze (grid);
			})
			.AddTo (this);

		update
			.Where (up => Input.GetKeyDown (KeyCode.W))
			.Subscribe (_ => movePlayer (0, 1, 0))
			.AddTo (this);
		update
			.Where (down => Input.GetKeyDown (KeyCode.S))
			.Subscribe (_ => movePlayer (0, -1, 180))
			.AddTo (this);
		update
			.Where (right => Input.GetKeyDown (KeyCode.D))
			.Subscribe (_ => movePlayer (1, 0, 90))
			.AddTo (this);
		update
			.Where (left => Input.GetKeyDown (KeyCode.A))
			.Subscribe (_ => movePlayer (-1, 0, 270))
			.AddTo (this);
		return;



	}
	void createGrid(){
		for (var y = 0; y < gridHeight; y++) {
			var row = new List<CellPresenter> ();
			for (var x = 0; x < gridWidth; x++) {
				var obj = Instantiate (cellPrefab, new Vector3 (x * (cellScale + cellSpan), 1, y * (cellScale + cellSpan)), Quaternion.identity) as GameObject;
				obj.transform.SetParent (gridContainer, true);
				obj.transform.localScale *= cellScale;
				var cp = obj.GetComponent<CellPresenter> ();
				row.Add (cp);
			}
			grid.Add (row);
		}
	}
	void initGrid(){
		rects.Clear();
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				var cp = grid [y] [x];
				var wall = 0;
				wall |= x == 0 ? CellPresenter.W : 0;
				wall |= x == gridWidth - 1 ? CellPresenter.E : 0;
				wall |= y == 0 ? CellPresenter.S : 0;
				wall |= y == gridHeight - 1 ? CellPresenter.N : 0;
				cp.wallBit.Value = wall;
				cp.bomb.Value = Random.value < .1f ? Random.Range(1,5) : 0;
				cp.visited.Value = 0;
			}
		}
		for (var y = 0; y < gridHeight; y++) {
			for (var x = 0; x < gridWidth; x++) {
				var cp = grid [y] [x];
				var neighbors = new List<CellPresenter> ();
				for (var ny = y - 1; ny <= y + 1; ny++) {
					for (var nx = x - 1; nx <= x + 1; nx++) {
						CellPresenter ncp;
						try{
							if(ny == y && nx == x){
								continue;
							}
							ncp = grid [ny] [nx];
						}
						catch {
							continue;
						}
						neighbors.Add (ncp);
					}
				}
				cp.neighborCells.Value = neighbors;
			}
		}
		rects.Add(new Rect(new Vector2(0,0), new Vector2(gridWidth, gridHeight)));

		playerX = (int)(gridWidth * .5f);
		playerY = (int)(gridHeight * .5f);
		movePlayer (0, 0, 0);

	}
	/*
	void addRoom(Rect r){
		for (int y = (int)r.yMin; y < (int)r.yMax; y++) {
			for (int x = (int)r.xMin; x < (int)r.xMax; x++) {
				var cell = 0;
				if (y == (int)r.yMin) {
					cell |= S;
				}
				else if (y == (int)r.yMax) {
					cell |= N;
				}
				if (x == (int)r.xMin) {
					cell |= W;
				}
				else if (x == (int)r.xMax) {
					cell |= E;
				}

				grid [y] [x] &= cell;
			}
		}		
	}
	*/
	void movePlayer(int x, int y, float rot)
	{
		playerX += x;
		playerY += y;
		player.transform.position = new Vector3 (playerX * ( cellScale + cellSpan), 2f, playerY * (cellScale + cellSpan));
		player.transform.rotation = Quaternion.Euler (0, rot, 0);
		CellPresenter cp;
		try{
			cp = grid [playerY] [playerX];
		}
		catch{
			return;
		}
		cp.visited.Value += 1;
		if (cp.bomb.Value > 0) {
			Debug.Log ("bomb:" + cp.bomb.Value);
			cp.bomb.Value--;
		}

	}
	int bitCount(int bit){
		int count;
		for (count = 0; bit != 0; bit >>= 1) {
			Debug.Log ("bit:" + bit);
			if ((bit & 1) != 0) {
				count++;
			}
		}
		return count;
	}
	/*
	void displayMaze(List<List<int>> grid){
		foreach (Transform t in gridContainer) {
			Destroy (t.gameObject);
		}
		for(var y = 0; y < grid.Count; y++){
			var row = grid [y];
			for(var x = 0; x < row.Count; x++){
				var cell = row [x];

				var obj = Instantiate (cellPrefab, new Vector3 (x * cellScale, 1, y * cellScale), Quaternion.identity) as GameObject;
				obj.transform.SetParent (gridContainer, true);
				obj.transform.localScale *= cellScale;
				var cp = obj.GetComponent<CellPresenter> ();
				cp.wallBit.Value = cell;
				cp.envBomb.Value = Random.Range (0, 9);


				if (cell == ALLDIR) {
					gridParent (Instantiate (blockPrefab, new Vector3 (x * cellScale, 1, y * cellScale), Quaternion.identity));

				} else {
					gridParent (Instantiate (floorPrefab, new Vector3 (x * cellScale, 0, y * cellScale), Quaternion.identity));
					if( (cell & E) != 0){
						gridParent (Instantiate (wallPrefab, new Vector3 ((x+.5f) * cellScale, 1, y * cellScale), Quaternion.identity));
					}
					if( (cell & S) != 0){
						gridParent (Instantiate (wallPrefab, new Vector3 (x *cellScale, 1, (y-.5f) * cellScale), Quaternion.Euler(0, 90, 0)));
					}
					if( (cell & W) != 0){
						gridParent (Instantiate (wallPrefab, new Vector3 ((x-.5f) * cellScale, 1, y * cellScale), Quaternion.identity));
					}
					if( (cell & N) != 0){
						gridParent (Instantiate (wallPrefab, new Vector3 (x * cellScale, 1, (y+.5f) * cellScale), Quaternion.Euler(0, 90, 0)));
					}
					if( (cell & BOMB) != 0){
//						gridParent (Instantiate (floorDivPrefab, new Vector3 (x * cellScale, 1, y * cellScale), Quaternion.identity));
					}

				}

			}
		}
		for(var i = 0; i < rects.Count; i++) {
			var r = rects [i];
//			var o = gridParent (Instantiate (floorPrefab, new Vector3 (r.xMin * cellScale, 0, r.yMin * cellScale), Quaternion.identity));
			for (int y = (int)r.yMin; y < (int)r.yMax; y++) {
				for (int x = (int)r.xMin; x < (int)r.xMax; x++) {
					gridParent (Instantiate (floorDivPrefab, new Vector3 (x * cellScale, 1, y * cellScale), Quaternion.identity));
						
				}
			}
		}
	}
*/
	GameObject gridParent(Object obj)
	{
		(obj as GameObject).transform.SetParent (gridContainer, true);
		(obj as GameObject).transform.localScale *= cellScale;
		return obj as GameObject;
	}
	bool div(List<Rect> rects){
		Rect rect;
		while (true) {
			if (rects.Count == 0) {
				return false;
			}

			rect = rects [0];
			rects.RemoveAt (0);

			var minSize = Random.Range(1,2);
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
				grid [wallY] [x].wallBit.Value |= CellPresenter.N;
				grid [wallY+1] [x].wallBit.Value |= CellPresenter.S;
			}
			var px = (int)Random.Range (rect.xMin, rect.xMax);
			passWall (px, wallY, CellPresenter.N);
			passWall (px, wallY+1, CellPresenter.S);
			if (rect.width > 5) {
				px = (px +(int)Random.Range (2, rect.width - 1)) % (int)rect.width;
				passWall (px, wallY, CellPresenter.N);
				passWall (px, wallY+1, CellPresenter.S);
			}

			rect1.yMax = wallY + 1;
			rect2.yMin = wallY + 1;
		} else {
			int wallX = (int)Mathf.Floor(Random.Range(rect.xMin, rect.xMax-2));
			for (int y = (int)rect.yMin; y < rect.yMax; y++) {
				grid [y] [wallX].wallBit.Value |= CellPresenter.E;
				grid [y] [wallX+1].wallBit.Value |= CellPresenter.W;
			}
			var py = (int)Random.Range (rect.yMin, rect.yMax);
			passWall (wallX, py, CellPresenter.E);
			passWall (wallX+1, py, CellPresenter.W);
			if (rect.height > 5) {
				py = (py +(int)Random.Range (2, rect.height - 1)) % (int)rect.height;
				passWall (wallX, py, CellPresenter.E);
				passWall (wallX+1, py, CellPresenter.W);
			}
			rect1.xMax = wallX + 1;
			rect2.xMin = wallX + 1;
		}
		rects.Insert (0, rect1);
		rects.Insert (0, rect2);
		return true;
	}

	void passWall(int x, int y, int dir)
	{
		grid[y][x].wallBit.Value &= (CellPresenter.ALLDIR ^ dir);
	}


}
