using UnityEngine;

public static class CameraUtils
{
    public static bool IsPointVisibleInCameraFOV(Vector3 point, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, new Bounds(point, Vector3.zero));
    }

    public static bool IsPointOccluded(GameObject point, Camera camera)
    {
        Vector3 direction = point.transform.position - camera.transform.position;
        Ray ray = new Ray(camera.transform.position, direction.normalized);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, direction.magnitude))
        {
            // Raycast hit something
            if (hit.collider != point.GetComponent<Collider>())
            {
                // The hit collider is not the sphere's collider, indicating occlusion
              //  Debug.Log("Hit is true");
                return true;
            }
            else
            {
                // The hit collider is the sphere's collider, meaning it's not occluded
              //  Debug.Log("Hit sphere itself");
                return false;
            }
        }
        else
        {
            // Raycast did not hit anything
           // Debug.Log("Raycast does not hit anything");
            return false;
        }
    }

    public static float DistanceToPointInPixels(Vector3 point, Camera camera)
    {
        Vector3 screenPoint = camera.WorldToScreenPoint(point);
        return Vector3.Distance(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), screenPoint);
    }
    public static float DistanceFromCamera(Vector3 point,Camera camera)
    {
        return Vector3.Distance(camera.transform.position, point);
    }
    public static float DistanceToPointInMM(Vector3 point, Camera camera)
    {
        float distanceInPixels = DistanceToPointInPixels(point,camera);
        float dpi = Screen.dpi == 0 ? 96f : Screen.dpi; // fallback DPI if unknown
        return distanceInPixels / dpi * 25.4f;
    }

    public static float? GetSurfaceAngleToCamera(Vector3 point,Camera camera,LayerMask layerMask)
    {
        Vector3 camPos = camera.transform.position;
        Vector3 pointPos = point;


        Vector3 cameraToPoint = (pointPos - camPos).normalized;
        Ray ray = new Ray(camPos, cameraToPoint);
        float maxDistance = camera.farClipPlane; // Limit ray distance to camera's view range
      //  Debug.Log(" Before Raycast");
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
        {
           // Debug.Log(" rayCast Hit"+ hit.transform.gameObject.name);

            // Ensure that the hit point is close enough to the Model Surface
            float distanceThreshold = 5f;
            if (Vector3.Distance(hit.point, pointPos) < distanceThreshold)
            {

                Vector3 surfaceNormal = hit.normal;


                float dotProduct = Vector3.Dot(surfaceNormal, cameraToPoint);
                dotProduct *= -1;

                dotProduct = Mathf.Clamp(dotProduct, -1f, 1f);


                return Mathf.Acos(dotProduct) * Mathf.Rad2Deg;


            }
            else
            {
                
                /// Raycasts  hits the surface of the 3D mesh, but far from the distance threshold from the point.
               


            }

        }
        return HoleSurfaceNormalEstimation();
       
    }
    public static float? HoleSurfaceNormalEstimation()
    {
        return null;
    }


}
