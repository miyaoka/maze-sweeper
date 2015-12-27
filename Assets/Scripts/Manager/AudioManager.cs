using UnityEngine;
using System.Collections;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
  public static AudioSource enemyDetect;
  public static AudioSource door;
  public static AudioSource walk;
  public static AudioSource maleScream;

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    AudioSource[] audios = GetComponents<AudioSource>();
    enemyDetect = audios[0];
    door = audios[1];
    walk = audios[2];
    maleScream = audios[3];
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
