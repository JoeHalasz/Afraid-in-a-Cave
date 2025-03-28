using UnityEngine;

public class MainCamera : MonoBehaviour
{

    void FixedUpdate()
    {
        // if Camera.allCameras has more than 1 camera then disable this objects camera and audio components
        // if the only camera is this one then enable this objects camera and audio components
        if (Camera.allCameras.Length > 1)
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            // make sure the camera is this camera
            if (Camera.allCameras.Length == 0 || Camera.allCameras[0] == GetComponent<Camera>())
            {
                GetComponent<Camera>().enabled = true;
                GetComponent<AudioListener>().enabled = true;
            }
        }
    }

}
