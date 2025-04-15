using UnityEngine;
using TMPro;

public class BillboardLabel : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textMeshPro;
    [SerializeField] private SpriteRenderer _backgroundSprite;
    [SerializeField] private float _paddingHorizontal;
    [SerializeField] private float _paddingVertical;

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError("No main camera found. Make sure your camera has the MainCamera tag.");
        }

        if (_textMeshPro == null)
        {
            _textMeshPro = GetComponentInChildren<TextMeshPro>();
        }

        if (_backgroundSprite == null)
        {
            _backgroundSprite = GetComponentInChildren<SpriteRenderer>();
        }

        Debug.Log(" From Start");
        UpdateBackgroundSize();
    }

    private void LateUpdate()
    {
        if (_mainCamera == null)
            return;

       
        transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward,
                        _mainCamera.transform.rotation * Vector3.up);
    }

    public void SetText(string text)
    {
        if (_textMeshPro != null)
        {
            _textMeshPro.text = text;
            UpdateBackgroundSize();
        }
    }

    private void UpdateBackgroundSize()
    {
        if (_textMeshPro == null || _backgroundSprite == null)
            return;

        _textMeshPro.ForceMeshUpdate();

        Bounds textBounds = _textMeshPro.mesh.bounds;

        // Get the local scale ratio between text and background
        // This handles cases where the parent is scaled
        float scaleRatioX = _backgroundSprite.transform.localScale.x / _textMeshPro.transform.localScale.x;
        float scaleRatioY = _backgroundSprite.transform.localScale.y / _textMeshPro.transform.localScale.y;

        float width = textBounds.size.x * scaleRatioX + _paddingHorizontal * 2f;
        float height = textBounds.size.y * scaleRatioY + _paddingVertical * 2f;

        Vector3 textCenter = textBounds.center;
        float offsetX = textCenter.x * scaleRatioX;
        float offsetY = textCenter.y * scaleRatioY;

      
        _backgroundSprite.size = new Vector2(width, height);

       
        _backgroundSprite.transform.localPosition = new Vector3(
            offsetX,
            offsetY,
            0.01f  // Slightly behind text
        );

       
        _backgroundSprite.sortingOrder = _textMeshPro.sortingOrder - 1;
    }


    private void OnValidate()
    {
        if (_textMeshPro != null && _backgroundSprite != null)
        {
            Debug.Log(" on validate");
            UpdateBackgroundSize();
        }
    }
}