using UnityEngine;

public class CenterMeshPivot : MonoBehaviour
{
    private Mesh _runtimeMesh; // Store the runtime mesh instance
    private CSVReader _reader;
    private Transform _pivotContainer;
    private CameraController _viewerController;
    private void Awake()
    {
        _reader= FindObjectOfType<CSVReader>();
        _viewerController= FindObjectOfType<CameraController>();
    }
    void InitializeMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter or Mesh is missing!");
            return;
        }

        // Create a new mesh instance for runtime
        _runtimeMesh = Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh = _runtimeMesh; // Assign the new mesh to the filter.

      
    }

    public void CenterPivot()
    {
        _reader.SpawnPoints();
         PivotCenter();
        _viewerController.UpdateModel(_pivotContainer);

    }
  
    void PivotCenter()
    {
        if (_runtimeMesh == null)
        {
            InitializeMesh(); 
        }



        Vector3 center = CalculateMeshCenter();

        // Create pivot container.
        _pivotContainer = new GameObject("PivotContainer").transform;
        _pivotContainer.SetParent(transform.parent);
        _pivotContainer.localPosition = new Vector3(-center.x, center.z,center.y); // Use localPosition here!
       
        _pivotContainer.localScale = transform.localScale;

        transform.SetParent(_pivotContainer);
       // transform.localPosition = -center;

    }
    Vector3 CalculateMeshCenter()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector3 center = Vector3.zero;

        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }
        return center / vertices.Length;
    }
}