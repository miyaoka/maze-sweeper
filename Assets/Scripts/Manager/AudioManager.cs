﻿using UnityEngine;
using System.Collections;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
  public static AudioSource EnemyDetect;
  public static AudioSource Door;
  public static AudioSource Walk;
  public static AudioSource MaleScream;
  public static AudioSource Powerup;

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    AudioSource[] audios = GetComponents<AudioSource>();
    EnemyDetect = audios[0];
    Door = audios[1];
    Walk = audios[2];
    MaleScream = audios[3];
    Powerup = audios[4];
  }
  public void PlayLoop(AudioSource au)
  {
    if(!au.loop)
    {
      au.loop = true;
      
      //Au also returns isPlaying if the time is end of clip, So double check.
      if(!au.isPlaying || au.time == au.clip.length)
      {
        au.Play();
      }
    }
  }
  public void StopLoop(AudioSource au)
  {
     au.loop = false;
//     au.Stop();
  }

}
