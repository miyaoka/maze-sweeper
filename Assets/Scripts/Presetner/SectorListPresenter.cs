using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using UniRx;

public class SectorListPresenter : MonoBehaviour
{
  [SerializeField]
  Button regenerateBtn;
  [SerializeField]
  GameObject sectorContainerPrefab;
  [SerializeField]
  GameObject sectorPrefab;

  SectorListManager list;

  void Start()
  {
    list = SectorListManager.Instance;

    list
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
        list.Init();
      })
      .AddTo(this);
  }

  void Init()
  {
    foreach(Transform t in transform)
    {
      Destroy(t.gameObject);
    }

    var sectorIndex = 1;
    list.SectorList.ForEach(sectorList =>
    {
      var sectorContainer = Instantiate(sectorContainerPrefab);
      sectorContainer.transform.SetParent(transform, false);

      var lvText = sectorContainer.GetComponentInChildren<Text>();
      lvText.text = string.Format("sector {0}", sectorIndex++);

      sectorList.ForEach(s =>
      {
        var sectorObj = Instantiate(sectorPrefab);
        sectorObj.transform.SetParent(sectorContainer.transform, false);

        sectorObj.GetComponent<SectorPresenter>().Sector = s;
      });
    });
  }
}
