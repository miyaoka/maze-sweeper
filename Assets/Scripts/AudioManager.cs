using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public static AudioSource enemyDetect;

	// Use this for initialization
	void Start () {
		AudioSource[] audios = GetComponents<AudioSource>();
		enemyDetect = audios[0];
	}

}
