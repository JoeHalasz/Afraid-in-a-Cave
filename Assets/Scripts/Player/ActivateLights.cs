using UnityEngine;
using Unity.Netcode;

public class ActivateLights : NetworkBehaviour
{
    // on trigger enter
    private void OnTriggerEnter(Collider other)
    {
        if (transform.parent.GetComponent<NetworkObject>().IsOwner)
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (transform.parent.GetComponent<NetworkObject>().IsOwner)
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
}
