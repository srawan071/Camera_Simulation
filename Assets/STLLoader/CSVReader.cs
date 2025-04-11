using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class BunnyOperationData
{
    public string FeatureID;
    public string Class;
    public float X;
    public float Y;
    public float Z;
    public string Type;
}

public class CSVReader : MonoBehaviour
{
    public List<BunnyOperationData> dataList = new List<BunnyOperationData>();
    [SerializeField] private InspectionSystem _pointManager;
    private void Start()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Bunny Operations Combined file.csv");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            // Skip the header
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                if (values.Length >= 6)
                {
                    BunnyOperationData data = new BunnyOperationData
                    {
                        FeatureID = values[0],
                        Class = values[1],
                        X = float.Parse(values[2]),
                        Y = float.Parse(values[3]),
                        Z = float.Parse(values[4]),
                        Type = values[5]
                    };

                    dataList.Add(data);
                }
            }

           
           
        }
        else
        {
            Debug.LogError("CSV file not found at path: " + filePath);
        }
       // PrintData();
       //SpawnPoints();
    }

   void PrintData()
    {
        foreach (var item in dataList)
        {
            Debug.Log($"Feature: {item.FeatureID} | Class: {item.Class} | X: {item.X} | Y: {item.Y} | Z: {item.Z} | Type: {item.Type}");
        }

    }

    public void SpawnPoints()
    {
        Vector3[] posData = dataList.Select(data => new Vector3(data.X, data.Y, data.Z)).ToArray();
        string [] nameData= dataList.Select(data => new string(data.FeatureID)).ToArray();
        _pointManager.SpawnPoints(dataList.Count, posData, nameData);
      
    }
}
