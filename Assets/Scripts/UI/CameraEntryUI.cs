using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _cameraNameText;
    [SerializeField] private TMP_Dropdown _cameraModelDropdown;
    [SerializeField] private TMP_Dropdown _lensModelDropdown;
    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Image _background;

    [HideInInspector] public Camera Camera;
    private CameraListPanel _listPanel;

    private Color _initialColor;

    private CameraModel _selectedCameraModel;
    private LensModel _selectedLensModel;

    public void Setup(string cameraName, Camera cameraObj, CameraListPanel panel)
    {
        _initialColor = _background.color;

        Camera = cameraObj;
        cameraObj.enabled = false;
        _listPanel = panel;
        _cameraNameText.text = cameraName;

        Camera.GetComponentInChildren<BillboardLabel>().SetText(cameraName);

        _selectButton.onClick.AddListener(() => _listPanel.SelectCamera(this));
        _deleteButton.onClick.AddListener(() => _listPanel.DeleteCamera(this));

        SetupDropdowns();
    }

    public void SetHighlight(bool isHighlighted)
    {
        _background.color = isHighlighted ? Color.yellow : _initialColor;
    }

    private void SetupDropdowns()
    {
        // Camera model dropdown
        _cameraModelDropdown.ClearOptions();
        List<string> cameraOptions = new List<string>(Enum.GetNames(typeof(CameraModel)));
        _cameraModelDropdown.AddOptions(cameraOptions);
        _cameraModelDropdown.onValueChanged.AddListener(OnCameraModelChanged);
        OnCameraModelChanged(0); // Initialize

        // Lens model dropdown
        _lensModelDropdown.ClearOptions();
        List<string> lensOptions = new List<string>(Enum.GetNames(typeof(LensModel)));
        _lensModelDropdown.AddOptions(lensOptions);
        _lensModelDropdown.onValueChanged.AddListener(OnLensModelChanged);
        OnLensModelChanged(0); // Initialize
    }

    private void OnCameraModelChanged(int index)
    {
        _selectedCameraModel = (CameraModel)index;
        ApplyToCameraCapture();
    }

    private void OnLensModelChanged(int index)
    {
        _selectedLensModel = (LensModel)index;
        ApplyToCameraCapture();
    }
    private void ApplyToCameraCapture()
    {
        var cameraCapture = Camera.GetComponent<CameraCapture>();
        if (cameraCapture != null)
        {
            cameraCapture.InitializeFromModels(_selectedCameraModel, _selectedLensModel);
        }
    }
}
