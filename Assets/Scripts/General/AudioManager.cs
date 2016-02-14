using UnityEngine;
using System.Collections;

public enum AudioName { EnemyDetect, Door, Walk, Scream, Powerup, Breach, Damage, TimeoutAlert, TimeoutDamage, EnergyLow, EnergyCritial }
public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
  AudioSource[] audios;

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    audios = GetComponents<AudioSource>();
  }
  public void Play(AudioName an)
  {
    getAudio(an).Play();
  }
  public void Stop(AudioName an)
  {
    getAudio(an).Stop();
  }
  AudioSource getAudio(AudioName an)
  {
    return audios[(int)an];
  }
  public void PlayLoop(AudioName an)
  {
    var au = getAudio(an);
    if (!au.loop)
    {
      au.loop = true;
      
      //Au also returns isPlaying if the time is end of clip, So double check.
      if(!au.isPlaying || au.time == au.clip.length)
      {
        au.Play();
      }
    }
  }
  public void StopLoop(AudioName an)
  {
    var au = getAudio(an);
    au.loop = false;
//     au.Stop();
  }

}
