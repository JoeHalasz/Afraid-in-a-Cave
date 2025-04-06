using UnityEngine;

public class ActivateLights : MonoBehaviour
{
    // on trigger enter
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Lights"))
        {
            Light light = other.gameObject.GetComponent<Light>();
            if (light != null)
                light.enabled = true;
            else
                Debug.LogWarning($"Light component not found on {other.gameObject.name}.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Lights"))
        {
            Light light = other.gameObject.GetComponent<Light>();
            if (light != null)
                light.enabled = false;
            else
                Debug.LogWarning($"Light component not found on {other.gameObject.name}.");
        }
    }
}
