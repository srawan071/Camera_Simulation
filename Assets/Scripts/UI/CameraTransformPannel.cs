using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraTramsformPannel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot; // Root GameObject of this panel to enable/disable
    public TMP_InputField posX, posY, posZ;
    public TMP_InputField rotYaw, rotPitch, rotRoll;

    private GameObject currentCamera;

    private Vector3 lastPosition;
    private Vector3 lastRotation;

    public void SetSelectedCamera(GameObject cam)
    {
        currentCamera = cam;
        panelRoot.SetActive(cam != null);

        if (cam != null)
        {
            lastPosition = cam.transform.position;
            lastRotation = cam.transform.eulerAngles;
            UpdateUIFromCamera();
        }
    }

    private void Update()
    {
        if (currentCamera == null) return;

        Vector3 currentPosition = currentCamera.transform.position;
        Vector3 currentRotation = currentCamera.transform.eulerAngles;

        if (currentPosition != lastPosition || currentRotation != lastRotation)
        {
            UpdateUIFromCamera();
            lastPosition = currentPosition;
            lastRotation = currentRotation;
        }
    }

    private void UpdateUIFromCamera()
    {
        Vector3 pos = currentCamera.transform.position;
        Vector3 rot = currentCamera.transform.eulerAngles;

        posX.text = pos.x.ToString("F2");
        posY.text = pos.y.ToString("F2");
        posZ.text = pos.z.ToString("F2");

        rotYaw.text = rot.y.ToString("F2");
        rotPitch.text = rot.x.ToString("F2");
        rotRoll.text = rot.z.ToString("F2");
    }

    public void OnPositionChanged()
    {
        if (currentCamera == null) return;

        if (float.TryParse(posX.text, out float x) &&
            float.TryParse(posY.text, out float y) &&
            float.TryParse(posZ.text, out float z))
        {
            currentCamera.transform.position = new Vector3(x, y, z);
        }
    }

    public void OnRotationChanged()
    {
        if (currentCamera == null) return;

        if (float.TryParse(rotYaw.text, out float yaw) &&
            float.TryParse(rotPitch.text, out float pitch) &&
            float.TryParse(rotRoll.text, out float roll))
        {
            currentCamera.transform.eulerAngles = new Vector3(pitch, yaw, roll);
        }
    }
}
