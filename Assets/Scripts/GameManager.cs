using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
public class GameManager : MonoBehaviour {

	[SerializeField] GameObject floorPrefab;
	[SerializeField] GameObject wallPrefab;
	[SerializeField] GameObject blockPrefab;
	[SerializeField] GameObject playerPrefab;
	[SerializeField] Transform gridContainer;
	[SerializeField] Transform camera;

	GameObject player;
	int mapWidth = 64;
	int mapHeight = 64;
	int gridWidth = 10;
	int gridHeight = 10;
	float cellScale = 4;
	List<List<int>> grid = new List<List<int>> ();
	// Use this for initialization
	void Start () {

		for (var y = 0; y < gridHeight; y++) {
			var row = new List<int> ();
			for (var x = 0; x < gridWidth; x++) {
				var cell = 0;
				cell |= x == 0 ? W : 0;
				cell |= x == gridWidth - 1 ? E : 0;
				cell |= y == 0 ? S : 0;
				cell |= y == gridHeight - 1 ? N : 0;
				row.Add (cell);
			}
			grid.Add (row);
		}

		divide (grid, 0, 0, 5, 5);
		displayMaze (grid);

		player = Instantiate (playerPrefab, new Vector3 (0, 2, 0), Quaternion.identity) as GameObject;



		var update = this
			.UpdateAsObservable ();

		update
			.Select (_ => player.transform.position)
			.Subscribe (pos => camera.position = new Vector3(pos.x, camera.position.y, pos.z))
			.AddTo (this);


		update
			.Where (up => Input.GetKeyDown (KeyCode.W))
			.Subscribe (_ => movePlayer (0, cellScale))
			.AddTo (this);
		update
			.Where (down => Input.GetKeyDown (KeyCode.S))
			.Subscribe (_ => movePlayer (0, -cellScale))
			.AddTo (this);
		update
			.Where (right => Input.GetKeyDown (KeyCode.D))
			.Subscribe (_ => movePlayer (cellScale, 0))
			.AddTo (this);
		update
			.Where (left => Input.GetKeyDown (KeyCode.A))
			.Subscribe (_ => movePlayer (-cellScale, 0))
			.AddTo (this);
		return;



	}
	void movePlayer(float x, float y)
	{
		player.transform.position += new Vector3 (x, 0, y);
	}

	const int E = 1;
	const int N = 2;
	const int W = 4;
	const int S = 8;
	void displayMaze(List<List<int>> grid){
		foreach (Transform t in gridContainer) {
			Destroy (t.gameObject);
		}
		for(var y = 0; y < grid.Count; y++){
			var row = grid [y];
			for(var x = 0; x < row.Count; x++){
				var cell = row [x];

				if (cell == 15) {
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
	}
	void gridParent(Object obj)
	{
		(obj as GameObject).transform.SetParent (gridContainer, true);
		(obj as GameObject).transform.localScale *= cellScale;
	}


	void divide(List<List<int>> grid, int startX, int startY, int endX, int endY){

		var minSize = 1;
		var width = endX - startX;
		var height = endY - startY;
		var isHorizontal = width == height ? Random.Range(0,2) == 0 : width < height;

		Debug.Log (isHorizontal +  "(" + startX + "," + startY + "), - (" + endX + "," + endY + ") "+ width + "/" + height);
//		Debug.Log (width + "," + height + "," + isHorizontal);
		if (width <= minSize || height <= minSize){
			return;
			/*
			int c, r;

			// make a hallway
			if (endX > 1) {
				r = startY;
				for (c = startX; c < endX; c++) {
					grid [r] [c] |= E;
					grid [r] [c + 1] |= W;
				}
			}
			else if(endY > 1){
				c = startX;
				for (r = startY; r < endY; r++){

					grid[r][c] |= S;
					grid[r+1][c] |= N;
				}
			}
			return;
			*/
		}

		//壁を作り、その中に通過地点を作る
		if (isHorizontal) {
			var wallY = Random.Range(startY, endY);
			for (var x = startX; x < endX; x++) {
				grid [wallY] [x] |= N;
			}
//			grid [wallY] [Random.Range(startX, endX)] ^= N;

			divide (grid, startX, startY, endX, wallY);
			divide (grid, startX, wallY + 1, endX, endY);

		} else {
			var wallX = Random.Range(startX, endX);
			for (var y = startY; y < endY; y++) {
				grid [y] [wallX] |= E;
			}
//			grid [Random.Range(startY, endY)] [wallX] ^= E;

			divide (grid, startX, startY, wallX, endY);
			divide (grid, wallX + 1, startY, endX, endY);
		}
	}


}
