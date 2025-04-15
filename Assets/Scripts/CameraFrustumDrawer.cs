using UnityEngine;

[ExecuteAlways]
public class CameraFrustumDrawer : MonoBehaviour
{
    [SerializeField] public Camera _inspectionCamera;
    [SerializeField] public Material _lineMaterial;
    [SerializeField] public Color _lineColor = Color.green;
    [SerializeField] private bool _showLineInGameView = true;
    [SerializeField] private bool _showlineInSceneView = true;

    void OnPostRender()
    {
        if (!_lineMaterial || !_inspectionCamera || !_showLineInGameView) return;
        DrawFrustumGL();
    }

    void OnDrawGizmos()
    {
        if (_inspectionCamera == null || !_showlineInSceneView) return;
        DrawFrustumGizmos();
    }

    void DrawFrustumGL()
    {
        _lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(_lineColor);

        DrawFrustumLines((a, b) =>
        {
            GL.Vertex(a);
            GL.Vertex(b);
        });

        GL.End();
        GL.PopMatrix();
    }

    void DrawFrustumGizmos()
    {
        Gizmos.color = _lineColor;
        DrawFrustumLines((a, b) => Gizmos.DrawLine(a, b));
    }

    void DrawFrustumLines(System.Action<Vector3, Vector3> drawLine)
    {
        if (_inspectionCamera == null) return;

        Camera cam = _inspectionCamera;

        Vector3[] nearCorners = new Vector3[4];
        Vector3[] farCorners = new Vector3[4];

        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);

        Transform camTransform = cam.transform;

        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = camTransform.TransformPoint(nearCorners[i]);
            farCorners[i] = camTransform.TransformPoint(farCorners[i]);
        }

        // Draw near plane
        drawLine(nearCorners[0], nearCorners[1]);
        drawLine(nearCorners[1], nearCorners[2]);
        drawLine(nearCorners[2], nearCorners[3]);
        drawLine(nearCorners[3], nearCorners[0]);

        // Draw far plane
        drawLine(farCorners[0], farCorners[1]);
        drawLine(farCorners[1], farCorners[2]);
        drawLine(farCorners[2], farCorners[3]);
        drawLine(farCorners[3], farCorners[0]);

        // Connect near to far
        for (int i = 0; i < 4; i++)
        {
            drawLine(nearCorners[i], farCorners[i]);
        }
    }
    public void SetSelectedInspectionCamera(Camera camera)
    {
       // Debug.Log(" Frustum Drawer "+ camera.name);
        _inspectionCamera = camera;
    }
}
