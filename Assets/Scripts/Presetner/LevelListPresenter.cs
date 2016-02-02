using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using UniRx;

public class LevelListPresenter : MonoBehaviour
{
  [SerializeField]
  Button regenerateBtn;
  [SerializeField]
  GameObject levelPrefab;
  [SerializeField]
  GameObject sectorPrefab;

  LevelListManager llm;

  void Start()
  {
    llm = LevelListManager.Instance;

    llm
      .Update
      .Subscribe(_ =>
      {
        Init();
      })
      .AddTo(this);


    regenerateBtn
      .OnClickAsObservable()
      .Subscribe(_ =>
      {
        LevelListManager.Instance.Init();
      })
      .AddTo(this);
  }

  void Init()
  {
    foreach(Transform t in transform)
    {
      Destroy(t.gameObject);
    }

    var levelIndex = 1;
    llm.LevelList.ForEach(sectorList =>
    {
      var lvObj = Instantiate(levelPrefab);
      lvObj.transform.SetParent(transform, false);

      var lvText = lvObj.GetComponentInChildren<Text>();
      lvText.text = string.Format("level {0}", levelIndex++);

      sectorList.ForEach(s =>
      {
        var sectorObj = Instantiate(sectorPrefab);
        sectorObj.transform.SetParent(lvObj.transform, false);

        sectorObj.GetComponent<SectorPresenter>().Sector = s;
      });
    });



  }

}
