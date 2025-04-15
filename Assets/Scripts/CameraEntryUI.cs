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

    [HideInInspector] public GameObject TargetCamera;
    private CameraListPanel listPanel;

    private Color _initialColor;

    public void Setup(string cameraName, GameObject cameraObj, CameraListPanel panel)
    {
        _initialColor= _background.color;

        TargetCamera = cameraObj;
        listPanel = panel;
        _cameraNameText.text = cameraName;

        TargetCamera.GetComponentInChildren<BillboardLabel>().SetText(cameraName);

        _selectButton.onClick.AddListener(() => listPanel.SelectCamera(this));
        _deleteButton.onClick.AddListener(() => listPanel.DeleteCamera(this));

        _cameraModelDropdown.ClearOptions();
        _cameraModelDropdown.AddOptions(new List<string> { "Model A", "Model B", "Model C", "Model D", "Model E", "Model F"});

        _lensModelDropdown.ClearOptions();
        _lensModelDropdown.AddOptions(new List<string> { "Lens W", "Lens X", "Lens Y", "Lens Z" });
    }

    public void SetHighlight(bool isHighlighted)
    {
        _background.color = isHighlighted ? Color.yellow : _initialColor;
    }
}
