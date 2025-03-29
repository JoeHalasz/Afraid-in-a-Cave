using UnityEngine;

public class PlayerView : MonoBehaviour
{

    GameObject camera;
    public float mouseSensitivity = .5f;

    void Start()
    {
        camera = transform.Find("Camera").gameObject;
    }

    void Update()
    {
        // rotate the camera up and down, and this object left and right
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0); // Rotate this object left and right

        // clamp the camera rotation to prevent flipping
        Vector3 cameraRotation = camera.transform.localEulerAngles;
        cameraRotation.x -= mouseY; // Rotate the camera up and down
        camera.transform.localEulerAngles = cameraRotation; // Apply the rotation to the camera

    }
}
