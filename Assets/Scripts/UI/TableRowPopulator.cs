using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TableRowPopulator : MonoBehaviour
{
    [SerializeField]
    private  TextMeshProUGUI[] _fields;

    [SerializeField]
    private Transform _targetTransform;

    
    private void Start()
    {
        SetBackGroundColor(transform.GetSiblingIndex()%2==0);
    }

    public void SetCameraRow(ResultsUIData.CameraData cam)
    {
        _fields[0].text = cam.id.ToString();
        _fields[1].text = cam.model;
        _fields[2].text = cam.lensModel;
        _fields[3].text = cam.position.ToString("F2");
        _fields[4].text = cam.rotation.ToString("F2");
    }

    public void SetFeatureRow(ResultsUIData.FeatureData f)
    {
        _fields[0].text = f.featureID.ToString();
        _fields[1].text = f.cameraID.ToString();
        _fields[2].text = f.worldCoord.ToString("F2");
        _fields[3].text = f.cameraCoord.ToString("F2");
        _fields[4].text = $"{f.distance:F2} mm";
        _fields[5].text = $"{f.surfaceAngle:F1}°";
        _fields[6].text = $"{f.pixelSize:F3} mm/pixel";
        _fields[7].text = f.isVisible.ToString();
        _fields[8].text = f.isOccluded.ToString();
        _fields[9].text = f.inFOV.ToString();
    }
    

    public void SetBackGroundColor(bool isEven)
    {
        Color color = isEven? Color.white : Color.HSVToRGB(0,0,.82f);
        transform.Cast<Transform>()
                 .Select(child => child.GetComponent<Image>())
                 .Where(image => image != null)
                 .ToList()
                 .ForEach(image => image.color = color);
    }
    private void Update()
    {
        if(_targetTransform != null)
        {
            transform.position= new Vector3(_targetTransform.position.x,transform.position.y,transform.position.z);
        }
    }
}
