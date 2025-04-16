using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
    private Camera _cameraPrefab; // Prefab with a Camera component

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
        Camera newCamera = Instantiate(_cameraPrefab, _defaultSpawnPos, Quaternion.identity);
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

        FocusCamera(entry.Camera);

        _selectedCameraPanel.SetSelectedCamera(entry.Camera.gameObject);

    }
    public void DeselectCamera()
    {
        if (selectedEntry != null)
        {
            selectedEntry.SetHighlight(false);

            Camera camComponent = selectedEntry.Camera;
            if (camComponent != null)
            {
                camComponent.targetTexture = null;
                camComponent.enabled = false;
                ClearRenderTexture();
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
        Destroy(entry.Camera.gameObject);
        Destroy(entry.gameObject);
    }

    private void FocusCamera(Camera selectedCamera)
    {
        bool anyCameraActive = false;

        foreach (var entry in cameraEntries)
        {
            Camera camComponent = entry.Camera;
            if (camComponent != null)
            {
                if (entry.Camera == selectedCamera)
                {
                    camComponent.targetTexture = _sharedRenderTexture;
                    camComponent.enabled = true;
                    anyCameraActive = true;
                    // Set selected camera in external controllers
                    _cameraController.SetSelectedInspectionCamera(camComponent);
                    _cameraFrustumDrawer.SetSelectedInspectionCamera(camComponent);
                }
                else
                {
                    camComponent.targetTexture = null;
                    camComponent.enabled = false;
                }
            }
        }
        if (!anyCameraActive)
        {
            ClearRenderTexture(); // Clear if no camera is active
        }
    }
    private void ClearRenderTexture()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = _sharedRenderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = currentRT;
    }

    public Camera[] GetAllCamera()
    {
        return cameraEntries.Select(entry => entry.Camera).ToArray();
    }
}
