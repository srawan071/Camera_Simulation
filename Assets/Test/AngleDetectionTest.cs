using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleDetectionTest : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform _camera;
    [SerializeField]
   private float _angle;
    [SerializeField]
    public bool _isVisible;
    [Range(0f, 180f)]
    [SerializeField]
    private float _visibilityThreshold;
    void Start()
    {
        _camera= Camera.main.transform;
    }

   
   
    private void Update()
    {
        GetAngle();
        //CalculateAngle();
    }
    void GetAngle()
    {
        Vector3 camPos = _camera.transform.position;
        Vector3 pointPos = transform.position;

       
        Vector3 cameraToPoint = (pointPos - camPos).normalized;
        Ray ray = new Ray(camPos, cameraToPoint);

        _isVisible = false;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Ensure that the hit point is close enough to the feature point
            float distanceThreshold = 0.01f;
            if (Vector3.Distance(hit.point, pointPos) < distanceThreshold)
            {
              
                Vector3 surfaceNormal = hit.normal;

               
                float dotProduct = Vector3.Dot(surfaceNormal, cameraToPoint);
                dotProduct *= -1;
               
                dotProduct = Mathf.Clamp(dotProduct, -1f, 1f);

             
                _angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                _isVisible = _angle < _visibilityThreshold;
            }
        }
    }

}
