using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region Data Structures

// Intrinsics and calibration parameters for the camera.
[System.Serializable]
public class CameraIntrinsicsValues
{
    public int width, height;
    public double fx, fy, ppx, ppy;
    public List<double> k;
    public double p1, p2;
    public double horizontalFOV, verticalFOV; // in degrees
    public double minWorkingDistance, maxWorkingDistance;
    public string cameraModel;
    public string lensModel;
    // Optionally, add more parameters as needed.
}

// Container for overall camera data which will be serialized.
[System.Serializable]
public class CameraData
{
    public CameraIntrinsicsValues cameraIntrinsics;
    public Vector3 position;
    public Quaternion rotation;
    public Matrix4x4 worldToCameraMatrix;
    // Optionally, you can include additional fields (like frame rate, sensor dimensions etc.)
}
#endregion

public class CameraCapture : MonoBehaviour
{
    public static int imagesTaken;
    string savedDataFolder = @"C:\temp";
    string ImageDataPath;
    CameraIntrinsicsValues intrinsics = new CameraIntrinsicsValues();

    // Selection of the camera and lens via Inspector.
    public CameraModel selectedCameraModel;
    public LensModel selectedLensModel;

    // These will be set from the chosen camera model.
    [SerializeField]int imageWidth, imageHeight;

    public Camera cameraObj;
    public GameObject gameObj;

    void Start()
    {
    }
    public void InitializeFromModels(CameraModel selectedCameraModel, LensModel selectedLensModel)
    {
        this.selectedCameraModel = selectedCameraModel;
        this.selectedLensModel = selectedLensModel;

        // Retrieve configuration data using the enums.
        CameraConfig camConfig = DeviceConfigManager.GetCameraConfig(selectedCameraModel);
        LensConfig lensConfig = DeviceConfigManager.GetLensConfig(selectedLensModel);

        // Update resolution from camera configuration.
        imageWidth = camConfig.resolutionWidth;
        imageHeight = camConfig.resolutionHeight;

        // Set intrinsic image size and principal point.
        intrinsics.width = imageWidth;
        intrinsics.height = imageHeight;
        intrinsics.ppx = imageWidth / 2.0;
        intrinsics.ppy = imageHeight / 2.0;

        // Compute focal length in pixel units:
        // fx = focal length (mm) / pixel size (mm)
        intrinsics.fx = lensConfig.focalLength / camConfig.pixelSize;
        intrinsics.fy = lensConfig.focalLength / camConfig.pixelSize;

        // Set distortion parameters to zero.
        intrinsics.p1 = 0.0;
        intrinsics.p2 = 0.0;
        intrinsics.k = new List<double> { 0, 0, 0 };

        // Calculate sensor dimensions in mm.
        double sensorWidth = imageWidth * camConfig.pixelSize;
        double sensorHeight = imageHeight * camConfig.pixelSize;

        // Calculate horizontal and vertical Field of View (FOV) in degrees.
        intrinsics.horizontalFOV = 2.0 * Mathf.Rad2Deg * (float)Math.Atan((sensorWidth / 2.0) / lensConfig.focalLength);
        intrinsics.verticalFOV = 2.0 * Mathf.Rad2Deg * (float)Math.Atan((sensorHeight / 2.0) / lensConfig.focalLength);

        // Set working distances from the lens configuration.
        intrinsics.minWorkingDistance = lensConfig.minWorkingDistance;
        intrinsics.maxWorkingDistance = lensConfig.maxWorkingDistance;

        // Store the selected model names.
        intrinsics.cameraModel = selectedCameraModel.ToString();
        intrinsics.lensModel = selectedLensModel.ToString();

        // Update the Unity camera parameters based on the intrinsic values.
        cameraObj.nearClipPlane = (float)intrinsics.minWorkingDistance;
        cameraObj.farClipPlane = (float)intrinsics.maxWorkingDistance;
        cameraObj.fieldOfView = (float)intrinsics.verticalFOV;

        // Compute and set the custom projection matrix.
        cameraObj.projectionMatrix = ComputeProjectionMatrix(cameraObj.projectionMatrix);

       /* Debug.Log("Camera Projection matrix:\n" + cameraObj.projectionMatrix);
        Debug.Log("Horizontal FOV: " + intrinsics.horizontalFOV + " degrees");
        Debug.Log("Vertical FOV: " + intrinsics.verticalFOV + " degrees");*/

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveImage(cameraObj, imageWidth, imageHeight);
            CreateCameraJson(cameraObj);
        }
    }

    public void SaveImage(Camera cam, int Width = 0, int Height = 0, string path = null)
    {
        int w = (Width == 0) ? imageWidth : Width;
        int h = (Height == 0) ? imageHeight : Height;
        ImageDataPath = (path == null) ? System.IO.Path.Combine(savedDataFolder, "image_" + imagesTaken) : path;

        if (!Directory.Exists(ImageDataPath))
        {
            Directory.CreateDirectory(ImageDataPath);
        }
        VRCameraCapture(cam, w, h);
    }

    public void VRCameraCapture(Camera cam, int width, int height)
    {
        // Save current render and camera states.
        RenderTexture prevRenderTexture = RenderTexture.active;
        RenderTexture prevCameraTargetTexture = cam.targetTexture;
        bool prevCameraEnabled = cam.enabled;

        int msaaSamples = 1;
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, msaaSamples);

        try
        {
            // Disable camera to capture a sharp image.
            cam.enabled = false;
            cam.targetTexture = renderTexture;
            cam.Render();
            RenderTexture.active = cam.targetTexture;

            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            byte[] bytes = screenShot.EncodeToPNG();
            string imageName = $"{cam.name}_{imagesTaken}.png";
            string imagePath = System.IO.Path.Combine(ImageDataPath, imageName);

            File.WriteAllBytes(imagePath, bytes);

            Debug.Log(string.Format("[<color=green>Screenshot</color>] Screenshot captured\n<color=grey>{0}</color>", imagePath));
            imagesTaken++;
        }
        catch (Exception ex)
        {
            Debug.LogError("Screenshot capture exception: " + ex);
        }
        finally
        {
            RenderTexture.ReleaseTemporary(renderTexture);
            RenderTexture.active = prevRenderTexture;
            cam.targetTexture = prevCameraTargetTexture;
            cam.enabled = prevCameraEnabled;
        }
    }

    void CreateCameraJson(Camera cam)
    {
        CameraData cameraData = GetCameraData(cam);

        string inputData = JsonUtility.ToJson(cameraData, true);
        File.WriteAllText(System.IO.Path.Combine(ImageDataPath, "CameraData.json"), inputData);
    }

    // Computes the custom projection matrix based on the intrinsic parameters.
    public Matrix4x4 ComputeProjectionMatrix(Matrix4x4 org)
    {
        // Use intrinsics.minWorkingDistance as the near clip distance.
        double near = intrinsics.minWorkingDistance;
        double cx = intrinsics.ppx;
        double cy = intrinsics.ppy;
        double fx = intrinsics.fx;
        double fy = intrinsics.fy;

        // Compute the frustum edges.
        double right = near * (imageWidth - cx) / fx;
        double left = near * (-cx) / fx;
        double top = near * (cy) / fy;
        double bottom = near * (cy - imageHeight) / fy;

        // Use the working distances from the intrinsics.
        float zNear = (float)intrinsics.minWorkingDistance;
        float zFar = (float)intrinsics.maxWorkingDistance;

        float x = (float)(2.0 * near / (right - left));
        float y = (float)(2.0 * near / (top - bottom));
        float a = (float)((right + left) / (right - left));
        float b = (float)((top + bottom) / (top - bottom));
        float c = -(zFar + zNear) / (zFar - zNear);
        float d = -(2.0f * zFar * zNear) / (zFar - zNear);

        Matrix4x4 mat = new Matrix4x4();
        mat.m00 = x; mat.m01 = 0; mat.m02 = a; mat.m03 = 0;
        mat.m10 = 0; mat.m11 = y; mat.m12 = b; mat.m13 = 0;
        mat.m20 = 0; mat.m21 = 0; mat.m22 = c; mat.m23 = d;
        mat.m30 = 0; mat.m31 = 0; mat.m32 = -1; mat.m33 = 0;

        return mat;
    }
    public CameraData GetCameraData(Camera cam)
    {
        CameraData cameraData = new CameraData();
        cameraData.cameraIntrinsics = intrinsics;

        cameraData.position = cam.transform.position;
        cameraData.rotation = cam.transform.rotation;
        cameraData.worldToCameraMatrix = cam.worldToCameraMatrix;
        return cameraData;
    }
}