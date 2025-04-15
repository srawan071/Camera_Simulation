using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.IO.Compression;

public class CameraCapture : MonoBehaviour
{
    public int imagesTaken;

    string savedDataFolder = @"C:\temp";
    string ImageDataPath;

    CameraIntrinsicsValues intrinsics = new CameraIntrinsicsValues();

    public int imageWidth, imageHeight;

    public GameObject gameObj;
    public Camera cameraObj;

    // Start is called before the first frame update
    void Start()
    {
        intrinsics.ppx = imageWidth / 2;
        intrinsics.ppy = imageHeight / 2;
        intrinsics.fx = 6700.0 * 2.0;
        intrinsics.fy = 6700.0 * 2.0;

        intrinsics.p1 = 0.0;
        intrinsics.p2 = 0.0;

        intrinsics.k = new List<double> { 0, 0, 0};

        cameraObj.projectionMatrix = projectionMatrix(cameraObj.projectionMatrix, cameraObj.nearClipPlane);

        Debug.Log(" Camera Projection matrix\n"+ cameraObj.projectionMatrix);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveImage(cameraObj,imageWidth,imageHeight);
            CreatCameraJson(cameraObj);
        }
    }

    public void SaveImage(Camera camera, int Width=0, int Height=0, string path=null)
    {
       
        Width= Width==0 ? imageWidth : Width; 
        Height= Height==0 ? imageHeight: Height;
        ImageDataPath =path==null? savedDataFolder + "\\image_" + imagesTaken: path;
       
        if (!Directory.Exists(ImageDataPath))
        {
            Directory.CreateDirectory(ImageDataPath);
        }
        //Debug.Log(camera);
        VRCameraCapture(camera, Width, Height);
    }
    public void VRCameraCapture(Camera camera, int width, int height)
    {

        // save data which we'll modify
        RenderTexture prevRenderTexture = RenderTexture.active;
        RenderTexture prevCameraTargetTexture = camera.targetTexture;
        bool prevCameraEnabled = camera.enabled;

        // create rendertexture
        int msaaSamples = 1;
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, msaaSamples);

        try
        {
            // disabling the camera is important, otherwise you get e. g. a blurry image with different focus than the one the camera displays
            // see https://docs.unity3d.com/ScriptReference/Camera.Render.html
            camera.enabled = false;

            // set rendertexture into which the camera renders
            camera.targetTexture = renderTexture;

            // render a single frame
            camera.Render();

            // create image using the camera's render texture
            RenderTexture.active = camera.targetTexture;

            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            // save the image
            byte[] bytes = screenShot.EncodeToPNG();
            string imageName = "image_" + imagesTaken + ".png";

            string imagePath = ImageDataPath + "\\" + imageName;

            System.IO.File.WriteAllBytes(imagePath, bytes);

            Debug.Log(string.Format("[<color=green>Screenshot</color>]Screenshot captured\n<color=grey>{0}</color>", imagePath));
            imagesTaken++;
        }
        catch (Exception ex)
        {
            Debug.LogError("Screenshot capture exception: " + ex);
        }
        finally
        {
            RenderTexture.ReleaseTemporary(renderTexture);

            // restore modified data
            RenderTexture.active = prevRenderTexture;
            camera.targetTexture = prevCameraTargetTexture;
            camera.enabled = prevCameraEnabled;

        }
    }
   
    void CreatCameraJson(Camera camera/*, Matrix4x4 transformation*/)
    {
        CameraData cameraData = new CameraData();
        cameraData.cameraIntrinsics = new CameraIntrinsicsValues();

        cameraData.cameraIntrinsics = intrinsics;

        cameraData.position = new Vector3();
        cameraData.rotation = new Quaternion();
        cameraData.worldToCameraMatrix = new Matrix4x4();
        //cameraData.transformationMatrix = new Matrix4x4();

        cameraData.position = camera.transform.position / 1000; // devide by 1000 to convert to meters
        cameraData.rotation = camera.transform.rotation;
        cameraData.worldToCameraMatrix = camera.worldToCameraMatrix;
        //cameraData.transformationMatrix = transformation;

        string inputData = JsonUtility.ToJson(cameraData, true);
        File.WriteAllText(ImageDataPath + "\\CameraData.json", inputData);
    }
   
    public Matrix4x4 projectionMatrix(Matrix4x4 org, double near)
    {
        var cx = intrinsics.ppx;
        var cy = intrinsics.ppy;
        var fx = intrinsics.fx;
        var fy = intrinsics.fy;

        var right = near * (imageWidth - cx) / fx;
        var left = near * (-cx) / fx;
        var top = near * (cy) / fy;
        var bottom = near * (cy - imageHeight) / fy;

        float zNear = (float)near;
        float zFar = cameraObj.farClipPlane;

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
        mat.m30 = 0; mat.m31 = 0; mat.m32 = -1f; mat.m33 = 0;

        return mat;
    }

    [System.Serializable]
    public class CameraData
    {
        public CameraIntrinsicsValues cameraIntrinsics;

        public Vector3 position;
        public Quaternion rotation;

        //public Matrix4x4 transformationMatrix;

        public Matrix4x4 worldToCameraMatrix;
    }
    [System.Serializable]
    public class CameraIntrinsicsValues
    {
        public int width, height;
        public double fx, fy, ppx, ppy;
        public List<double> k;
        public double p1, p2;
        public string cameraModel;
        public string cameraSerialNumber;
        public string lensModel;
    }
}
