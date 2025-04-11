
using UnityEngine;
using UnityEngine.UI;

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


    void Start()
    {
        if (_cameraTransform == null)
            _cameraTransform = Camera.main.transform;

       _rotateAroundToggle.onValueChanged.AddListener(OnToggleRotateAround);
        _cameraToggle.onValueChanged.AddListener(OnToggleCamera);
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
        HandlePan();
    }
    public void OnToggleCamera(bool toggle)
    {
        _cameraTransform= toggle?_inspectionCamera.transform: _mainCamera.transform;
       // Debug.Log($" Toggle value is "+ toggle + " camera transform"+ _cameraTransform);
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
}


