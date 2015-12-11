using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

	public static AudioSource enemyDetect;
	public static AudioSource door;
	public static AudioSource walk;

	// Use this for initialization
	void Awake () {
		AudioSource[] audios = GetComponents<AudioSource>();
		enemyDetect = audios[0];
		door = audios[1];
		walk = audios[2];
	}

}
