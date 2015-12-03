using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FloorPresenter : MonoBehaviour {
	[SerializeField] RawImage floorB_LT;
	[SerializeField] RawImage floorB_RT;
	[SerializeField] RawImage floorB_LB;
	[SerializeField] RawImage floorB_RB;
	[SerializeField] RawImage floorW_LT;
	[SerializeField] RawImage floorW_RT;
	[SerializeField] RawImage floorW_LB;
	[SerializeField] RawImage floorW_RB;

	const int DIR_T = 1 << 0;
	const int DIR_R = 1 << 1;
	const int DIR_B = 1 << 2;
	const int DIR_L = 1 << 3;
	const int DIR_ALL = DIR_T | DIR_R | DIR_B | DIR_L;

	void Start () {
		buildFloor ();
	}
	
	void buildFloor(){

		var lb = floorTile (0, DIR_ALL);
		int reqDir;
		int availableDir;


		reqDir = 0;
		availableDir = DIR_ALL;
		if ((lb & DIR_R) != 0) {
			reqDir |= DIR_L;
		} else {
			availableDir ^= DIR_L;
		}
		var rb = floorTile (reqDir, availableDir);

		reqDir = 0;
		availableDir = DIR_ALL;
		if ((lb & DIR_T) != 0) {
			reqDir |= DIR_B;
		} else {
			availableDir ^= DIR_B;
		}
		var lt = floorTile (reqDir, availableDir);

		reqDir = 0;
		availableDir = DIR_ALL;
		if ((rb & DIR_T) != 0) {
			reqDir |= DIR_B;
		} else {
			availableDir ^= DIR_B;
		}
		if ((lt & DIR_R) != 0) {
			reqDir |= DIR_L;
		} else {
			availableDir ^= DIR_L;
		}
		var rt = floorTile (reqDir, availableDir);

		/*
		Debug.Log (binaryToString (lt) + "-" + binaryToString (rt));
		Debug.Log (binaryToString (lb) + "-" + binaryToString (rb));

		Debug.Log (binaryToString (reqDir));
		Debug.Log (binaryToString (availableDir));
*/
		var xDiff1 = Random.value;
		var yDiff1 = Random.value;
		var xDiff2 = Random.value;
		var yDiff2 = Random.value;
		floorB_LT.uvRect = floorW_LT.uvRect = uvRect(lt, xDiff1, yDiff1);
		floorB_RT.uvRect = floorW_RT.uvRect = uvRect(rt, xDiff2, yDiff1);
		floorB_LB.uvRect = floorW_LB.uvRect = uvRect(lb, xDiff1, yDiff2);
		floorB_RB.uvRect = floorW_RB.uvRect = uvRect(rb, xDiff2, yDiff2);
	}
	// x 1/64: 
	// y 1/4 .25: .05-0.45
	Rect uvRect(int index, float xDiff, float yDiff){
		return new Rect (
			(float)index / 16f + 1f / 64f, .05f + yDiff * .4f,
			1f / 32f, .5f
		);
	}
	int floorTile(int requireDir, int availableDir){
		int tile = 0;
		while (tile == 0) {
			tile = (Random.Range (0, 1 << 4) | requireDir) & availableDir;
			//2辺に満たなければ空欄にしてやり直し
			if (bitCount (tile) < 2) {
				tile = 0;
			}
			//必須辺が無ければ空欄でも可にする
			if (requireDir == 0) {
				break;
			}
		}
		return tile;
	}
	int bitCount(int bit){
		int count;
		for (count = 0; bit != 0; bit >>= 1) {
			if ((bit & 1) != 0) {
				count++;
			}
		}
		return count;
	}
	string binaryToString(int i){
		return System.Convert.ToString (i, 2).PadLeft (4, '0');
	}
}
