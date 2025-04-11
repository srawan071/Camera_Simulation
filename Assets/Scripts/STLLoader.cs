using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System;

public class UniversalSTLLoader : MonoBehaviour
{
    [SerializeField] string _fileName = "Bunny part.stl";
    [SerializeField] private Material _material;
    [SerializeField] private float _scale;
   
    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, _fileName);
        Debug.Log($"[STLLoader] Trying to load: {path}");

        if (!File.Exists(path))
        {
            Debug.LogError($"[STLLoader] File not found at {path}");
            return;
        }

        if (IsBinarySTL(path))
        {
            Debug.Log("[STLLoader] Detected Binary STL file.");
            Mesh mesh = LoadBinarySTL(path);
            CreateMeshObject(mesh, "BinarySTLObject");
        }
        else
        {
            Debug.Log("[STLLoader] Detected ASCII STL file.");
            Mesh mesh = LoadASCIIStl(path);
            CreateMeshObject(mesh, "ASCIISTLObject");
        }
    }

    bool IsBinarySTL(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        // Check if header starts with "solid"
        string header = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(80, bytes.Length));
        if (header.Trim().StartsWith("solid"))
        {
            // Heuristic: look for "facet" and "vertex" as additional confirmation
            string content = File.ReadAllText(path);
            return !(content.Contains("facet") && content.Contains("vertex"));
        }

        return true;
    }

    Mesh LoadASCIIStl(string path)
    {
        try
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            string[] lines = File.ReadAllLines(path);
            int vertIndex = 0;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("vertex"))
                {
                    string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    vertices.Add(new Vector3(x, y, z));
                }

                if (vertices.Count % 3 == 0 && vertices.Count > 0)
                {
                    int baseIndex = vertIndex * 3;
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                    vertIndex++;
                }
            }

            if (vertices.Count < 3)
            {
                Debug.LogError("[STLLoader] ASCII STL file did not contain enough vertices.");
                return null;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
        catch (Exception ex)
        {
            Debug.LogError("[STLLoader] ASCII load error: " + ex.Message);
            return null;
        }
    }

    Mesh LoadBinarySTL(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            int triangleCount = BitConverter.ToInt32(bytes, 80);

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            int offset = 84;
            for (int i = 0; i < triangleCount; i++)
            {
                offset += 12; // skip normal

                for (int v = 0; v < 3; v++)
                {
                    float x = BitConverter.ToSingle(bytes, offset);
                    float y = BitConverter.ToSingle(bytes, offset + 4);
                    float z = BitConverter.ToSingle(bytes, offset + 8);
                    vertices.Add(new Vector3(x, y, z));
                    offset += 12;
                }

                offset += 2; // skip attribute byte count

                int baseIndex = i * 3;
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
        catch (Exception ex)
        {
            Debug.LogError("[STLLoader] Binary load error: " + ex.Message);
            return null;
        }
    }

    void CreateMeshObject(Mesh mesh, string name)
    {
        if (mesh == null)
        {
            Debug.LogError("[STLLoader] Mesh is null, cannot create object.");
            return;
        }

        // Center the mesh around origin
        Vector3[] verts = mesh.vertices;
        Vector3 center = Vector3.zero;

        foreach (var v in verts)
            center += v;

        center /= verts.Length;

        for (int i = 0; i < verts.Length; i++)
            verts[i] -= center;

        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Create GameObject
        GameObject obj = new GameObject(name);
        obj.transform.position = Vector3.zero;

        var mf = obj.AddComponent<MeshFilter>();
        var mr = obj.AddComponent<MeshRenderer>();

        mf.mesh = mesh;
        mr.material = _material;

        // Optional: scale down if it's huge
        obj.transform.localScale = Vector3.one * _scale;

        Debug.Log($"[STLLoader] Created centered mesh with {mesh.vertexCount} vertices.");

       
    }

}
