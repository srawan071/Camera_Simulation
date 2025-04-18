using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class InspectionSystem : MonoBehaviour
{
    [SerializeField] public GameObject[] _points; // Assign spheres in the Inspector
    [SerializeField] private List<PointData> _pointDataList = new List<PointData>();
    [SerializeField] private GameObject _pointPrefab;
    [SerializeField] private Transform _model;
    [SerializeField] private LayerMask _modelLayer;
    [SerializeField] private CameraListPanel _cameraList;

    [SerializeField] private ResultsUIManager _resultsUIManager; // Drag & assign in Inspector
    private List<ResultsUIData.CameraData> _cameraResults = new List<ResultsUIData.CameraData>();
    private List<ResultsUIData.FeatureData> _featureResults = new List<ResultsUIData.FeatureData>();

    private Dictionary<Camera, CameraCapture> _cameraCapture= new Dictionary<Camera, CameraCapture>();

    private Camera _inspectionCamera;
    private string _savedDataFolder = @"C:\temp";
    private int _resultCount;



    private void Awake()
    {
        _resultCount = PlayerPrefs.GetInt("RESULT");
    }
    public void CalculatePointData()
    {
        Camera[] allCameras = InitializeDataAndGetCameras();

        Dictionary<GameObject, AggregatedPointData> aggregatedData = InitializeAggregationData();

        foreach (Camera cam in allCameras)
        {
            ProcessCamera(cam, aggregatedData);
        }

        ApplyFinalPointColors(aggregatedData);
        UpdateUIAndResultCount();
    }

    

    private Camera[] InitializeDataAndGetCameras()
    {
        _cameraResults.Clear();
        _featureResults.Clear();
        _cameraCapture.Clear();

        Camera[] allCameras = _cameraList.GetAllCamera();
        foreach (Camera cam in allCameras)
        {
            _cameraCapture[cam] = cam.GetComponent<CameraCapture>();
        }

        return allCameras;
    }


    private Dictionary<GameObject, AggregatedPointData> InitializeAggregationData()
    {
        var aggregatedData = new Dictionary<GameObject, AggregatedPointData>();
        foreach (var point in _points)
        {
            aggregatedData[point] = new AggregatedPointData();
        }
        return aggregatedData;
    }

    private void ProcessCamera(Camera cam, Dictionary<GameObject, AggregatedPointData> aggregatedData)
    {
        _inspectionCamera = cam;
        _pointDataList.Clear();

        AddCameraResult(cam);

        foreach (var point in _points)
        {
            ProcessPoint(point, cam, aggregatedData);
        }

        ShowPointData(cam.name);
        SaveInspectionCameraImage(cam);
    }

    private void AddCameraResult(Camera cam)
    {
        var camData = _cameraCapture[cam].GetCameraData(cam);

        _cameraResults.Add(new ResultsUIData.CameraData
        {
            id = cam.name,
            model = camData.cameraIntrinsics.cameraModel,
            lensModel = camData.cameraIntrinsics.lensModel,
            position = cam.transform.position,
            rotation = cam.transform.eulerAngles
        });
    }

    private void ProcessPoint(GameObject point, Camera cam, Dictionary<GameObject, AggregatedPointData> aggregatedData)
    {
        Vector3 worldPos = point.transform.position;
        Vector3 cameraPos = cam.transform.InverseTransformPoint(worldPos);

        bool inFOV = CameraUtils.IsPointVisibleInCameraFOV(worldPos, cam);
        bool occluded = CameraUtils.IsPointOccluded(point, cam);
        bool visible = inFOV && !occluded;

        float? angle = CameraUtils.GetSurfaceAngleToCamera(worldPos, cam, _modelLayer);
        float distanceMM = CameraUtils.DistanceToPointInMM(worldPos, cam);
        Vector2 pixelsAtPoint = _cameraCapture[cam].GetPixelsAtPoint(cameraPos.z);

        // Aggregate data
        var data = aggregatedData[point];
        if (inFOV) data.InFOV = true;
        if (visible) data.IsVisibleInAny = true;
        if (angle != null) data.HasNonNullAngle = true;

        // Store feature data to display in UI
        _featureResults.Add(new ResultsUIData.FeatureData
        {
            featureID = point.name,
            cameraID = cam.name,
            worldCoord = worldPos,
            cameraCoord = cameraPos,
            distance = distanceMM,
            surfaceAngle = angle.ToString(),
            pixelSize = pixelsAtPoint,
            isVisible = visible,
            isOccluded = occluded,
            inFOV = inFOV
        });

        //store point/ feature date to save in json file.
        _pointDataList.Add(new PointData
        {
            FeatureID = point.name,
            PositionInWorldCordinate = worldPos,
            PositionInCameraCordinate = cameraPos,
            IsVisible = visible,
            IsOccluded = occluded,
            IsInFov = inFOV,
            SurfaceAngle = angle.ToString(),
            DistanceToPointMM = distanceMM
        });

        SetColor(point, inFOV, occluded, angle);
    }

    private void ApplyFinalPointColors(Dictionary<GameObject, AggregatedPointData> aggregatedData)
    {
        foreach (var kvp in aggregatedData)
        {
            GameObject point = kvp.Key;
            var data = kvp.Value;

            bool finalInFOV = data.InFOV;
            bool finalVisible = data.IsVisibleInAny;
            bool finalHasAngle = data.HasNonNullAngle;

            bool finalOccluded = finalInFOV && !finalVisible;
            float? dummyAngle = finalHasAngle ? 45f : (float?)null;

            SetColor(point, finalInFOV, finalOccluded, dummyAngle);
        }
    }

    private void UpdateUIAndResultCount()
    {
        _resultsUIManager.PopulateCameraSummary(_cameraResults);
        _resultsUIManager.PopulateFeatureSummary(_featureResults);
        UpdateResultCount();
    }

    void SetColor(GameObject point, bool inFov, bool isOccluded,float? angle)
    {
        var renderer = point.GetComponent<Renderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            //renderer.material.color = Color.red;
            propertyBlock.SetColor("_Color", Color.red);

            if (inFov)
            {
               // renderer.material.color= Color.green;
                propertyBlock.SetColor("_Color", Color.green);

                if (isOccluded)
                {
                   // renderer.material.color = Color.blue;
                    propertyBlock.SetColor("_Color", Color.blue);

                }
                else if (angle == null)
                {
                   // renderer.material.color = Color.yellow;
                    propertyBlock.SetColor("_Color", Color.yellow);

                }
            }
           
            renderer.SetPropertyBlock(propertyBlock);
           
        }
    }

  

    public void SpawnPoints(int count, Vector3[] pos, string[] name)
    {
        _points = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(_pointPrefab, pos[i], Quaternion.identity);
            go.transform.SetParent(_model, true);
            go.name = name[i];
            _points[i] = go;
        }

    }

    public void ShowPointData(string camName)
    {
        CameraData camData = _cameraCapture[_inspectionCamera].GetCameraData(_inspectionCamera);
       
        Vector3 CameraRot = new Vector3(
        WrapAngle(_inspectionCamera.transform.eulerAngles.x),
        WrapAngle(_inspectionCamera.transform.eulerAngles.y),
        WrapAngle(_inspectionCamera.transform.eulerAngles.z));

        
        string json = JsonUtility.ToJson(new PointDataListWrapper
        {
           cameraData= camData,
            points = _pointDataList

        }, true);

        SaveJson(json,$"{camName}_result.json");
        // Log the JSON string
        Debug.Log($" Result is {json}");
    }
    private void SaveJson(string json, string filename = "inspection_result.json")
    {
        
        string path = Path.Combine(GetPath(), filename);
        File.WriteAllText(path, json);
        Debug.Log($"JSON saved to: {path}");
       
    }
    public void SaveMainCameraImage()
    {
        FindObjectOfType<CameraCapture>().SaveImage(Camera.main,Screen.width,Screen.height,GetPath());
    }
    public void SaveInspectionCameraImage(Camera cam)
    {
        cam.transform.GetComponent<CameraCapture>().SaveImage(_inspectionCamera,path:GetPath());
    }
    private string GetPath()
    {
        string path = Path.Combine(_savedDataFolder, "_results", _resultCount.ToString());
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
      
    }
    private void UpdateResultCount()
    {
        _resultCount++;
        PlayerPrefs.SetInt("RESULT", _resultCount);
    }
    float WrapAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }
    [System.Serializable]
    public class PointDataListWrapper
    {
        public CameraData cameraData;
        public List<PointData> points;

    }

    private class AggregatedPointData
    {
        public bool InFOV = false;
        public bool HasNonNullAngle = false;
        public bool IsVisibleInAny = false;
        public bool AllAnglesNull = true;
    }
}
