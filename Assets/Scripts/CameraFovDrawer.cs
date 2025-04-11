using UnityEngine;

[ExecuteAlways]
public class CameraFOVDrawer : MonoBehaviour
{
    [SerializeField]
    public Camera _inspectionCamera;
    [SerializeField]
    public Material _lineMaterial;
    [SerializeField]
    public Color _lineColor = Color.green;
    [SerializeField]
    private bool _showLineInGameView = true;
    [SerializeField]
    private bool _showlineInSceneView = true;

    void OnPostRender()
    {
        if (!_lineMaterial || !_inspectionCamera || !_showLineInGameView) return;

        DrawFrustumGLLines();
    }

    void DrawFrustumGLLines()
    {
        _lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(_lineColor);

        Vector3[] corners = GetFrustumCorners(_inspectionCamera);

        Vector3 camPos = _inspectionCamera.transform.position;

        // Lines from camera to corners
        foreach (var corner in corners)
        {
            GL.Vertex(camPos);
            GL.Vertex(corner);
        }

        // Connect corners around far plane
        DrawLine(corners[0], corners[1]);
        DrawLine(corners[1], corners[2]);
        DrawLine(corners[2], corners[3]);
        DrawLine(corners[3], corners[0]);

        GL.End();
        GL.PopMatrix();
    }

    void OnDrawGizmos()
    {
        if (_inspectionCamera == null|| !_showlineInSceneView) return;

        Gizmos.color = _lineColor;
        Vector3[] corners = GetFrustumCorners(_inspectionCamera);
        Vector3 camPos = _inspectionCamera.transform.position;

        foreach (var corner in corners)
            Gizmos.DrawLine(camPos, corner);

        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
    }

    Vector3[] GetFrustumCorners(Camera cam)
    {
        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float far = cam.farClipPlane;

        float halfFovRad = fov * 0.5f * Mathf.Deg2Rad;
        float h = Mathf.Tan(halfFovRad) * far;
        float w = h * aspect;

        Vector3 camPos = cam.transform.position;
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        Vector3 up = cam.transform.up;

        Vector3 centerFar = camPos + forward * far;

        Vector3 topLeft = centerFar + (up * h) - (right * w);
        Vector3 topRight = centerFar + (up * h) + (right * w);
        Vector3 bottomLeft = centerFar - (up * h) - (right * w);
        Vector3 bottomRight = centerFar - (up * h) + (right * w);

        return new Vector3[] { topLeft, topRight, bottomRight, bottomLeft };
    }

    void DrawLine(Vector3 a, Vector3 b)
    {
        GL.Vertex(a);
        GL.Vertex(b);
    }
}
