using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultsUIManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject _cameraRowPrefab;
    [SerializeField]
    private GameObject _featureRowPrefab;

    [Header("Parents")]
    [SerializeField]
    private Transform _cameraTableParent;
    [SerializeField]
    private Transform _featureTableParent;

    public void PopulateCameraSummary(List<ResultsUIData.CameraData> cameras)
    {
        foreach (Transform child in _cameraTableParent) Destroy(child.gameObject);

        foreach (var cam in cameras)
        {
            var row = Instantiate(_cameraRowPrefab, _cameraTableParent);
            row.GetComponent<TableRowPopulator>().SetCameraRow(cam);
        }
    }

    public void PopulateFeatureSummary(List<ResultsUIData.FeatureData> features)
    {
        foreach (Transform child in _featureTableParent) Destroy(child.gameObject);

        foreach (var feature in features)
        {
            var row = Instantiate(_featureRowPrefab, _featureTableParent);
            row.GetComponent<TableRowPopulator>().SetFeatureRow(feature);
        }
    }
}
