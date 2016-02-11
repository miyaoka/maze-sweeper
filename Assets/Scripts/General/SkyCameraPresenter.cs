using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class SkyCameraPresenter : MonoBehaviour
{
  [SerializeField]
  bool randomRotation = false;
  [SerializeField]
  float speed = .01f;

  void Start()
  {
    if(randomRotation)
    {
      RandomRotate();
    }
    this
      .UpdateAsObservable()
      .Subscribe(_ => transform.rotation *= Quaternion.Euler(new Vector3(0, -speed, 0)))
      .AddTo(this);
  }
  public void RandomRotate()
  {
    transform.rotation = Random.rotation;
  }
}
