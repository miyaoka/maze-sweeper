using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
public class GameManager : MonoBehaviour {

	const int E = 1 << 0;
	const int N = 1 << 1;
	const int W = 1 << 2;
	const int S = 1 << 3;
	const int ALLDIR = N | E | W | S;

	[SerializeField] GameObject floorPrefab;
	[SerializeField] GameObject floorDivPrefab;
	[SerializeField] GameObject wallPrefab;
	[SerializeField] GameObject blockPrefab;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] Transform gridContainer;
	[SerializeField] Transform camera;

	GameObject player;
	int gridWidth = 30;
	int gridHeight = 30;
	float cellScale = 2;
	List<List<int>> grid = new List<List<int>> ();
	List<Rect> rects = new List<Rect> ();

	// Use this for initialization
	void Start () {

		for (var y = 0; y < gridHeight; y++) {
			var row = new List<int> ();
			for (var x = 0; x < gridWidth; x++) {
				row.Add (0);
			}
			grid.Add (row);
		}


//		divide (grid, 0, 0, 5, 5);
//		displayMaze (grid);

		player = Instantiate (playerPrefab, new Vector3 (gridWidth * cellScale * .5f, 1 * cellScale, gridHeight * cellScale * .5f), Quaternion.identity) as GameObject;


		var update = this
			.UpdateAsObservable ();

		update
			.Select (_ => player.transform.position)
			.Subscribe (pos => camera.position = new Vector3(pos.x, camera.position.y, pos.z))
			.AddTo (this);

//		displayMaze (grid);

		update
			.Where (up => Input.GetKey (KeyCode.Space))
			.Subscribe (_ => {

				//				div(rects);
				while(div(rects)){
				}
				displayMaze (grid);
			})
			.AddTo (this);
		update
			.Where (up => Input.GetKey (KeyCode.R))
			.Subscribe (_ => {

				addRoom(new Rect(Random.Range(0, gridWidth-10),Random.Range(0,gridHeight-10),Random.Range(3,10),Random.Range(3,10)));
				displayMaze (grid);
			})
			.AddTo (this);
		update
			.Where (up => Input.GetKeyDown (KeyCode.Backspace))
			.Subscribe (_ => {
				rects.Clear();
				for (var y = 0; y < gridHeight; y++) {
					for (var x = 0; x < gridWidth; x++) {
						var cell = 0;

						cell |= x == 0 ? W : 0;
						cell |= x == gridWidth - 1 ? E : 0;
						cell |= y == 0 ? S : 0;
						cell |= y == gridHeight - 1 ? N : 0;
						grid[y][x] = cell;
					}
				}
				rects.Add(new Rect(new Vector2(0,0), new Vector2(gridWidth, gridHeight)));
				displayMaze (grid);
			})
			.AddTo (this);

		update
			.Where (up => Input.GetKey (KeyCode.W))
			.Subscribe (_ => movePlayer (0, 1, 0))
			.AddTo (this);
		update
			.Where (down => Input.GetKey (KeyCode.S))
			.Subscribe (_ => movePlayer (0, -1, 180))
			.AddTo (this);
		update
			.Where (right => Input.GetKey (KeyCode.D))
			.Subscribe (_ => movePlayer (1, 0, 90))
			.AddTo (this);
		update
			.Where (left => Input.GetKey (KeyCode.A))
			.Subscribe (_ => movePlayer (-1, 0, 270))
			.AddTo (this);
		return;



	}
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
	void movePlayer(float x, float y, float rot)
	{
		player.transform.position += new Vector3 (x * cellScale * .5f, 0, y * cellScale * .5f);
		player.transform.rotation = Quaternion.Euler (0, rot, 0);
	}

	void displayMaze(List<List<int>> grid){
		foreach (Transform t in gridContainer) {
			Destroy (t.gameObject);
		}
		for(var y = 0; y < grid.Count; y++){
			var row = grid [y];
			for(var x = 0; x < row.Count; x++){
				var cell = row [x];

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

				}

			}
		}
		for(var i = 0; i < rects.Count; i++) {
			var r = rects [i];
//			var o = gridParent (Instantiate (floorPrefab, new Vector3 (r.xMin * cellScale, 0, r.yMin * cellScale), Quaternion.identity));
			for (int ry = (int)r.yMin; ry < (int)r.yMax; ry++) {
				for (int rx = (int)r.xMin; rx < (int)r.xMax; rx++) {
				gridParent (Instantiate (floorDivPrefab, new Vector3 (rx * cellScale, 1, ry * cellScale), Quaternion.identity));
						
				}
			}
//			*/
		}
	}
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
			if (rect.width <=  Random.Range(1,5) && rect.height <=  Random.Range(1,5)) {
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
				grid [wallY] [x] |= N;
				grid [wallY+1] [x] |= S;
			}
			var px = (int)Random.Range (rect.xMin, rect.xMax);
			passWall (px, wallY, N);
			passWall (px, wallY+1, S);
			rect1.yMax = wallY + 1;
			rect2.yMin = wallY + 1;
		} else {
			int wallX = (int)Mathf.Floor(Random.Range(rect.xMin, rect.xMax-2));
			for (int y = (int)rect.yMin; y < rect.yMax; y++) {
				grid [y] [wallX] |= E;
				grid [y] [wallX+1] |= W;
			}
			var py = (int)Random.Range (rect.yMin, rect.yMax);
			passWall (wallX, py, E);
			passWall (wallX+1, py, W);
			rect1.xMax = wallX + 1;
			rect2.xMin = wallX + 1;
		}
		rects.Insert (0, rect1);
		rects.Insert (0, rect2);
		return true;
	}

	void passWall(int x, int y, int dir)
	{
		grid[y][x] &= (ALLDIR ^ dir);
	}


}
