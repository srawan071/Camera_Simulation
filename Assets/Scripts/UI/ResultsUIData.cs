using UnityEngine;
[System.Serializable]
public class ResultsUIData 
{

[System.Serializable]
public class CameraData
{
    public string id;
    public string model;
    public string lensModel;
    public Vector3 position;
    public Vector3 rotation;
}

[System.Serializable]
public class FeatureData
{
    public string featureID;
    public string cameraID;
    public Vector3 worldCoord;
    public Vector3 cameraCoord;
    public float distance;
    public string surfaceAngle;
    public Vector2 pixelSize;
    public bool isVisible;
    public bool isOccluded;
    public bool inFOV;
}

}
