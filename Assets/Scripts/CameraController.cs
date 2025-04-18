
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform _model;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private float _rotationSpeed = 200f;
    [SerializeField]
    private float _zoomSpeed = 2f;
    [SerializeField]
    private float _panSpeed = 5f; // Added pan speed

    
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private Camera _inspectionCamera;
    [SerializeField]
    private Toggle _cameraToggle;
    [SerializeField]
    private Toggle _rotateAroundToggle;
    [SerializeField]
    private bool _rotateAroundOrigin;

    [SerializeField]
    private GameObject _3DViewTab;

    void Start()
    {
        if (_cameraTransform == null)
            _cameraTransform = Camera.main.transform;

       _rotateAroundToggle.onValueChanged.AddListener(OnToggleRotateAround);
        _cameraToggle.onValueChanged.AddListener(OnToggleCamera);
    }

    void Update()
    {
        if (!Is3dViewActive())
            return;
        // Prevent camera movement if pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        HandleRotation();
        HandleZoom();
        HandlePan();
    }
    public void OnToggleCamera(bool toggle)
    {
        if(_inspectionCamera == null&& toggle)
        {
            _cameraToggle.isOn = false;
            return;
        }
        _cameraTransform= toggle?_inspectionCamera.transform: _mainCamera.transform;
       
    }
    public void OnToggleRotateAround(bool toggle)
    {
        _rotateAroundOrigin = toggle;
    }
    void HandleRotation()
    {
        if (Input.GetMouseButton(0)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
           
            if (Mathf.Abs(mouseX) < 5f && Mathf.Abs(mouseY) < 5f)
            {
                Vector3 pointToRotateAround = _rotateAroundOrigin ? _cameraTransform.position : _model.position;

               
                _cameraTransform.RotateAround(pointToRotateAround, Vector3.up, -mouseX * _rotationSpeed * Time.deltaTime);

               
                _cameraTransform.RotateAround(pointToRotateAround, _cameraTransform.right, mouseY * _rotationSpeed * Time.deltaTime);
            }
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
           
            _cameraTransform.position = _cameraTransform.position + _cameraTransform.forward * -scroll * _zoomSpeed;
        }
    }


    void HandlePan()
    {
        if (Input.GetMouseButton(1)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (Mathf.Abs(mouseX) < 5f && Mathf.Abs(mouseY) < 5f)
            {
                // Pan the camera based on mouse movement (move the camera in the plane of the screen)
                Vector3 pan = _cameraTransform.right * -mouseX + _cameraTransform.up * -mouseY;
                _cameraTransform.position += pan * _panSpeed * Time.deltaTime;
            }
        }
    }

    public void UpdateModel(Transform model)
    {
        _model = model;
    }
    private void OnDestroy()
    {
        _cameraToggle.onValueChanged.RemoveListener(OnToggleCamera);
        _rotateAroundToggle.onValueChanged.RemoveListener(OnToggleRotateAround);
    }
    public void SetSelectedInspectionCamera(Camera camera)
    {
        _inspectionCamera = camera;
        if (_inspectionCamera != null)
        {
            _cameraToggle.SetIsOnWithoutNotify(false);
            _cameraToggle.isOn = true; // This will now fire the event
        }
        else
        {
            _cameraToggle.isOn = false;
        }

    }
    private bool Is3dViewActive()
    {
        return _3DViewTab.activeInHierarchy;
    }
}


