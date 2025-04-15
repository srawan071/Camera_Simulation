using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraListPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] 
    private Button _addCameraButton;
    [SerializeField] 
    private GameObject _cameraEntryPrefab;
    [SerializeField] 
    private Transform _cameraListContent;
    [SerializeField] 
    private CameraTramsformPannel _selectedCameraPanel;
    [SerializeField]
    private RenderTexture _sharedRenderTexture;


    [Header("Settings")]
    [SerializeField]
    private GameObject _cameraPrefab; // Prefab with a Camera component

    private int cameraCount = 0;
    private List<CameraEntryUI> cameraEntries = new List<CameraEntryUI>();
    private CameraEntryUI selectedEntry;
    [SerializeField]
    private CameraController _cameraController;
    [SerializeField]
    private CameraFrustumDrawer _cameraFrustumDrawer;
    [SerializeField]
    private Vector3 _defaultSpawnPos;
   

    private void Start()
    {
        _addCameraButton.onClick.AddListener(AddCamera);
    }

    private void AddCamera()
    {
        cameraCount++;

       // Vector3 defaultPosition = new Vector3(cameraCount * 2, 1, 0); // offset placement
        GameObject newCamera = Instantiate(_cameraPrefab, _defaultSpawnPos, Quaternion.identity);
        newCamera.name = $"Camera {cameraCount}";

        GameObject entry = Instantiate(_cameraEntryPrefab, _cameraListContent);
        CameraEntryUI entryUI = entry.GetComponent<CameraEntryUI>();
        entryUI.Setup(newCamera.name, newCamera, this);

        cameraEntries.Add(entryUI);
    }

    public void SelectCamera(CameraEntryUI entry)
    {
        if (selectedEntry == entry)
        {
            // Camera is already selected, so deselect it
            DeselectCamera();
            return;
        }

        if (selectedEntry != null)
            selectedEntry.SetHighlight(false);

        selectedEntry = entry;
        selectedEntry.SetHighlight(true);

        FocusCamera(entry.TargetCamera);

        _selectedCameraPanel.SetSelectedCamera(entry.TargetCamera);

    }
    public void DeselectCamera()
    {
        if (selectedEntry != null)
        {
            selectedEntry.SetHighlight(false);

            Camera camComponent = selectedEntry.TargetCamera.GetComponent<Camera>();
            if (camComponent != null)
            {
                camComponent.targetTexture = null;
            }

            _selectedCameraPanel.SetSelectedCamera(null);
            _cameraController.SetSelectedInspectionCamera(null);
            _cameraFrustumDrawer.SetSelectedInspectionCamera(null);

            selectedEntry = null;
        }
    }


    public void DeleteCamera(CameraEntryUI entry)
    {
        if (selectedEntry == entry)
        {
            _selectedCameraPanel.SetSelectedCamera(null);
            _cameraController.SetSelectedInspectionCamera(null);
            _cameraFrustumDrawer.SetSelectedInspectionCamera(null);
            selectedEntry = null;

        }
       

        cameraEntries.Remove(entry);
        Destroy(entry.TargetCamera);
        Destroy(entry.gameObject);
    }

    private void FocusCamera(GameObject selectedCamera)
    {
        foreach (var entry in cameraEntries)
        {
            Camera camComponent = entry.TargetCamera.GetComponent<Camera>();
            if (camComponent != null)
            {
                if (entry.TargetCamera == selectedCamera)
                {
                    camComponent.targetTexture = _sharedRenderTexture;

                    // Set selected camera in external controllers
                    _cameraController.SetSelectedInspectionCamera(camComponent);
                    _cameraFrustumDrawer.SetSelectedInspectionCamera(camComponent);
                }
                else
                {
                    camComponent.targetTexture = null;
                }
            }
        }
    }
}
