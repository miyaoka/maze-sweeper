﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using Random = UnityEngine.Random;
using AssetBundles;

[Prefab("BGMManager")]
public class BGMManager : SingletonMonoBehaviour<BGMManager> {
  List<string> titleList = new List<string>() {
    "Aliens",
    "Amnesia 2", //dance /mid
    "Can You Really Fly", //ok /fast
//    "Da Mihi Factum 3",
//    "Deserted Land",
//    "Drown",
    "Drown 2", //dance /fast
    "End Of Time",//@ 緊迫 /mid
    "Faded Photographs 2",
    "Meteor", //@ ホラー調、中盤から盛り上がり
    "Mysterious Universe",
    "Rise To The Top",
    "Running Wild",
    "Scramble",
    "Stand Firm",
    "Taken", //勇ましい /mid
//    "They Are Here", //ホラー調
    "They're Coming For Us", //ok /mid
    "Time Chasers",//緊迫 /fast
    "Triangulum", //@ 緊迫 /fast
//    "Walking On Sunshine", //陽気
//    "War Of The Planets", //前半切る 派手すぎ
    "Orion",
    "Galactic Mission"
  };

  string bundleBasePath = "https://dl.dropboxusercontent.com/u/1030861/unity/elpis/AssetBundles/";
  AudioSource aus;
  public Button musicBtn;
  public Button clearBtn;
  public Text titleText;
  ReactiveProperty<int> currentIndex = new ReactiveProperty<int>(-1);
  bool inited = false;
  string queue;

  IEnumerator Start() {
    aus = GetComponent<AudioSource>();

    currentIndex
      .Where(i => 0 <= i)
      .Select(i => titleList[i])
      .Subscribe(t =>
      {
        Play(t);
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

    yield return StartCoroutine(Initialize());
    inited = true;
    if (queue != null)
    {
      Play(queue);
    }

  }
  // Initialize the downloading url and AssetBundleManifest object.
  protected IEnumerator Initialize()
  {
    // Don't destroy this gameObject as we depend on it to run the loading script.
    DontDestroyOnLoad(gameObject);

    // With this code, when in-editor or using a development builds: Always use the AssetBundle Server
    // (This is very dependent on the production workflow of the project. 
    // 	Another approach would be to make this configurable in the standalone player.)
//#if DEVELOPMENT_BUILD || UNITY_EDITOR
    AssetBundleManager.SetDevelopmentAssetBundleServer();
//#else
		// Use the following code if AssetBundles are embedded in the project for example via StreamingAssets folder etc:
//		AssetBundleManager.SetSourceAssetBundleURL(Application.dataPath + "/");
		// Or customize the URL based on your deployment or configuration
		//AssetBundleManager.SetSourceAssetBundleURL("http://www.MyWebsite/MyAssetBundles");
//#endif

    // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
    var request = AssetBundleManager.Initialize();
    if (request != null)
      yield return StartCoroutine(request);
  }
  public void Play(string bgmName, bool loop = true)
  {
    if (!inited)
    {
      queue = bgmName;
      return;
    }
    aus.loop = loop;
    titleText.text = "- loading -";
    StartCoroutine(InstantiateAssetAsync("bgm", bgmName));
    titleText.text = bgmName;
  }
  /*
  public void Play(string bgmName)
  {
    var i = titleList.IndexOf(bgmName);
    if (i < 0 || i == currentIndex.Value)
    {
      return;
    }
    currentIndex.Value = i;
  }
  */
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
  IEnumerator InstantiateAssetAsync(string assetBundleName, string assetName)
  {
    // This is simply to get the elapsed time for this phase of AssetLoading.
    float startTime = Time.realtimeSinceStartup;

    // Load asset from assetBundle.
    AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(AudioClip));
    if (request == null)
      yield break;
    yield return StartCoroutine(request);

    // Get the asset.
    var asset = request.GetAsset<AudioClip>();
    aus.clip = asset;
    aus.Play();

    // Calculate and display the elapsed time.
    float elapsedTime = Time.realtimeSinceStartup - startTime;
    Debug.Log(assetName + (asset == null ? " was not" : " was") + " loaded successfully in " + elapsedTime + " seconds");
  }
}