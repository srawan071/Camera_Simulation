using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
public class InspectionSystem : MonoBehaviour
{
    [SerializeField] public GameObject[] _points; // Assign spheres in the Inspector
    [SerializeField] private List<PointData> _pointDataList = new List<PointData>();
    [SerializeField] private GameObject _pointPrefab;
    [SerializeField] private Transform _model;
    [SerializeField] private LayerMask _modelLayer;
    [SerializeField] private CameraListPanel _cameraList;

    private Camera _inspectionCamera;
    private string _savedDataFolder = @"C:\temp";
    private int _resultCount;

    private void Awake()
    {
        _resultCount = PlayerPrefs.GetInt("RESULT");
    }
    public void CalculatePointData()
    {
        Camera[] allcameras= _cameraList.GetAllCamera();
        foreach (Camera cam in allcameras)
        {
            _inspectionCamera= cam;
            _pointDataList.Clear();
            foreach (var point in _points)
            {
                Vector3 positionInWorldSpace = point.transform.position;
                Vector3 positionInCameraSpace = _inspectionCamera.transform.InverseTransformPoint(positionInWorldSpace);



                bool inFOV = CameraUtils.IsPointVisibleInCameraFOV(positionInWorldSpace, _inspectionCamera);
                bool occluded = CameraUtils.IsPointOccluded(point, _inspectionCamera);
                bool visible = inFOV && !occluded;

                float? angle = CameraUtils.GetSurfaceAngleToCamera(positionInWorldSpace, _inspectionCamera, _modelLayer);
                float distanceMM = CameraUtils.DistanceToPointInMM(positionInWorldSpace, _inspectionCamera);

                _pointDataList.Add(new PointData
                {
                    PositionInWorldCordinate = positionInWorldSpace,
                    PositionInCameraCordinate = positionInCameraSpace,
                    IsVisible = visible,
                    IsOccluded = occluded,
                    IsInFov = inFOV,
                    SurfaceAngle = angle.ToString(),
                    DistanceToPointMM = distanceMM
                });

                SetColor(point, inFOV, occluded, angle);
            }


            ShowPointData(_inspectionCamera.name);
            SaveInspectionCameraImage(_inspectionCamera);
        }

        UpdateResultCount();
    }

    void SetColor(GameObject point, bool inFov, bool isOccluded,float? angle)
    {
        var renderer = point.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
            if (inFov)
            {
                renderer.material.color= Color.green;
                if (isOccluded)
                {
                    renderer.material.color = Color.blue;
                }
               else if (angle == null)
                {
                    renderer.material.color = Color.yellow;

                }
            }
           
            
           
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
        CameraData camData = _inspectionCamera.transform.GetComponent<CameraCapture>().GetCameraData(_inspectionCamera);
       
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
   
    
}
