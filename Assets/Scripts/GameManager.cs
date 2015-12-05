using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
public class GameManager : SingletonMonoBehaviour<GameManager>{
	[SerializeField] GameObject playerPrefab;
	[SerializeField] Slider slider;


	public ReactiveProperty<int> alertCount = new ReactiveProperty<int>();
	public ReactiveProperty<int> enemyCount = new ReactiveProperty<int>();

	GridManager gridManager;
	void Awake ()
	{
		if (this != Instance) {
			Destroy (this);
			return;
		}
		DontDestroyOnLoad (this.gameObject);
	}
	void Start(){
		gridManager = GetComponent<GridManager> ();

		StartCoroutine (levelStart ());
	}

	IEnumerator gameLoop(){
		yield return 1;
	}
	IEnumerator levelStart(){
		gridManager.initGrid ();
		yield return 1;

	}
	IEnumerator levelPlaying(){
		yield return 1;
	}
	IEnumerator levelEnd(){
		yield return 1;
	}
	IEnumerator battleStart(){
		yield return 1;
	}
	IEnumerator battlePlaying(){
		yield return 1;
	}
	IEnumerator battleEnd(){
		yield return 1;
	}



}