using UnityEngine;

[System.Serializable]
public class PointData
{
    public string FeatureID;
    public Vector3 PositionInWorldCordinate;
    public Vector3 PositionInCameraCordinate;
    public bool IsInFov;
    public bool IsOccluded;
    public bool IsVisible;
    public string SurfaceAngle;
    public float DistanceToPointMM;
}
