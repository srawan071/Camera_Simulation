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
        var left = near * (1 - cx) / fx;
        var bottom = near * (1 - cy) / fy;
        var top = near * (imageHeight - cy) / fy;

        var x = (float)(2 * near / (right - left));
        var y = (float)(2 * near / (top - bottom));
        var a = (float)((right + left) / (right - left));
        var b = (float)((top + bottom) / (top - bottom));

        //float fov = 2.0f * Mathf.Atan(1.0f / y) * 180.0f / Mathf.PI;
        //Debug.Log(fov);


        org.m00 = x;
        org.m02 = a;
        org.m11 = y;
        org.m12 = b;

        return org;
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
