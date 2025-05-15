using UnityEngine;
using Unity.Netcode;

public class PlayerView : NetworkBehaviour
{

    GameObject cam;
    public float mouseSensitivity = .5f;

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        cam = transform.Find("Camera").gameObject;
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        // rotate the camera up and down, and this object left and right
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0); // Rotate this object left and right

        // clamp the camera rotation to prevent flipping
        Vector3 cameraRotation = cam.transform.localEulerAngles;
        cameraRotation.x -= mouseY; // Rotate the camera up and down
        
        if (cameraRotation.x > 90 && cameraRotation.x < 270)
        {
            // if we are closer to 270 than 90, set it to 270
            if (cameraRotation.x > 180)
                cameraRotation.x = 270;
            else
                cameraRotation.x = 90;
        }
        cam.transform.localEulerAngles = cameraRotation; // Apply the rotation to the camera
    }
}
