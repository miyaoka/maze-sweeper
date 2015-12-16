using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;
[RequireComponent(typeof(Camera))]
public class SkyCameraPresenter : MonoBehaviour {
  [SerializeField] bool randomRotation = false;
  [SerializeField] float speed = .01f;

	void Start () {
    if (randomRotation) {
      randomRotate ();
    }
    this
      .UpdateAsObservable ()
      .Subscribe (_ => transform.rotation *= Quaternion.Euler (new Vector3 (0, -speed, 0)))
      .AddTo (this);
	}
  public void randomRotate(){
    transform.rotation = Random.rotation;
  }
}
