using UnityEngine;

public class Flashlight : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // this object is the flashlight
            // toggle the flashlight on and off
            Light flashlight = GetComponent<Light>();
            if (flashlight != null)
            {
                flashlight.enabled = !flashlight.enabled;
            }
            else
            {
                Debug.LogError("Flashlight component not found on this object.");
            }
        }
    }
}
