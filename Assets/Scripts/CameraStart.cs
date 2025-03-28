using UnityEngine;

public class CameraStart : MonoBehaviour
{

    void Start()
    {
        // remove the mouse
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

}
