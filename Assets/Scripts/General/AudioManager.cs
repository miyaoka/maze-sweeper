using UnityEngine;
using System.Collections;
using UniRx;
using System;
using Random = UnityEngine.Random;
using System.Linq;

public enum AudioName { EnemyDetect, Door, Walk, Scream, Powerup, Breach, Damage, TimeoutAlert, TimeoutDamage, EnergyLow, EnergyCritial }
public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
  AudioSource[] audios;
  [SerializeField]
  Transform footstepContainer;

  bool onPlayFootStep;
  AudioSource[] footsteps;
  AudioSource footstepAu;
  int fi = 0;
  IDisposable footstepTimer = null;

  void Awake()
  {
    if(this != Instance)
    {
      Destroy(this);
      return;
    }
    audios = GetComponents<AudioSource>();

    footsteps = footstepContainer.GetComponents<AudioSource>();
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
  public void PlayFootstep()
  {
    if (onPlayFootStep)
    {
      return;
    }

    onPlayFootStep = true;
    playFootstep();
  }
  void playFootstep(int? lastIndex = null)
  {
    var footstepList =
      footstepContainer
      .GetComponents<AudioSource>()
      .Where(a => a.enabled)
      .ToList();

    var i = lastIndex.HasValue
      ? (lastIndex.Value + Random.Range(1, footstepList.Count)) % footstepList.Count
      : Random.Range(0, footstepList.Count);

    footstepAu = footstepList[i];
//    footstepAu.volume = .8f;
//    footstepAu.pitch = .5f;
    footstepAu.Stop();
    footstepAu.Play();

    
    footstepTimer =
    Observable
      .Timer(TimeSpan.FromSeconds(footstepAu.clip.length + Random.Range(.02f, .04f)))
      .Subscribe(_ => {
        footstepTimer.Dispose();
        playFootstep(i);
      });

  }
  public void StopFootStep()
  {
    onPlayFootStep = false;
    if(footstepAu != null)
    {
//      footstepAu.Stop();
    }
    if(footstepTimer != null)
    {
      footstepTimer.Dispose();
    }
  }
}
