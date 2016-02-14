using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using Random = UnityEngine.Random;

[Prefab("BGMManager")]
public class BGMManager : SingletonMonoBehaviour<BGMManager> {
  List<string> titleList = new List<string>() {
    "Aliens",
    "Amnesia 2",
    "Can You Really Fly",
    "Da Mihi Factum 3",
    "Deserted Land",
    "Drown",
    "Drown 2",
    "End Of Time",
    "Faded Photographs 2",
    "Meteor",
    "Mysterious Universe",
    "Rise To The Top",
    "Running Wild",
    "Scramble",
    "Stand Firm",
    "Taken",
    "They Are Here",
    "They're Coming For Us",
    "Time Chasers",
    "Triangulum",
    "Walking On Sunshine",
    "War Of The Planets",
    "In the Rain"
  };

  string bundleBasePath = "https://dl.dropboxusercontent.com/u/1030861/unity/elpis/AssetBundles/";
  AudioSource aus;
  public Button musicBtn;
  public Button clearBtn;
  public Text titleText;
  ReactiveProperty<int> currentIndex = new ReactiveProperty<int>(-1);

  void Start() {
    DontDestroyOnLoad(gameObject);
    aus = GetComponent<AudioSource>();

    currentIndex
      .Where(i => 0 <= i)
      .Select(i => titleList[i])
      .Subscribe(t =>
      {
          titleText.text = t;
          StartCoroutine(DownloadAndCache("bgm", t));
      })
      .AddTo(this);
    currentIndex
      .Where(i => 0 > i)
      .Subscribe(_ => titleText.text = "no BGM")
      .AddTo(this);

    musicBtn
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
          currentIndex.Value = (currentIndex.Value + 1) % titleList.Count;
      })
      .AddTo(this);

    clearBtn
      .OnClickAsObservable()
      .Subscribe(_ => StartCoroutine(ClearCache()))
      .AddTo(this);
  }
  public void Play(string bgmName)
  {
    var i = titleList.IndexOf(bgmName);
    if (i < 0 || i == currentIndex.Value)
    {
      return;
    }
    currentIndex.Value = i;
  }
  public void RandomPlay(){
    currentIndex.Value = (currentIndex.Value + Random.Range(1, titleList.Count)) % titleList.Count;
  }
  public void Stop(){
    aus.Stop();
    currentIndex.Value = -1;
  }
  IEnumerator ClearCache ()
  {
    // Wait for the Caching system to be ready
    while (!Caching.ready)
    {
      yield return null;
    }
    Caching.CleanCache();
  }
  IEnumerator DownloadAndCache (string bundleName, string assetName, int version = 1)
  {
    // Wait for the Caching system to be ready
    while (!Caching.ready)
    {
      yield return null;
    }
    var url = bundleBasePath + bundleName;
    // Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
    using (WWW www = WWW.LoadFromCacheOrDownload(url, version))
    {
      yield return www;
      if (www.error != null)
      {
        throw new Exception("WWW download had an error:" + www.error);
      }
      var bundle = www.assetBundle;

//      bundle.GetAllAssetNames().ToList().ForEach(t => Debug.Log(t));

      // Load the object asynchronously
      AssetBundleRequest request = bundle.LoadAssetAsync (assetName, typeof(AudioClip));
      // Wait for completion
      yield return request;

      aus.clip = request.asset as AudioClip;
      aus.Play();

      // Unload the AssetBundles compressed contents to conserve memory
      bundle.Unload(false);

      // Frees the memory from the web stream
      www.Dispose();    
    }
  }
}