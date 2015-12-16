using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
[RequireComponent(typeof(Camera))]
public class SkyCameraPresenter : MonoBehaviour {
  [SerializeField] bool randomRotation = false;
  [SerializeField] float speed = .01f;

	// Use this for initialization
	void Start () {
    if (randomRotation) {
      transform.rotation = Random.rotation;
      Debug.Log (transform.rotation.eulerAngles);
    }
    this
      .UpdateAsObservable ()
      .Subscribe (_ => transform.rotation *= Quaternion.Euler (new Vector3 (0, -speed, 0)))
      .AddTo (this);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
