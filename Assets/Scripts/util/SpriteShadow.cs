using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteShadow : MonoBehaviour
{

  [SerializeField]
  ShadowCastingMode shadow = ShadowCastingMode.On;

  void Awake()
  {
    GetComponent<SpriteRenderer>().shadowCastingMode = shadow;
  }
}
