using UnityEngine;
using Unity.Netcode;

public class Flashlight : NetworkBehaviour
{
    GameObject flashlight;

    [SerializeField]
    GameObject flashlightPrefab;

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            // if it doesnt exist create it and turn on its network object
            if (flashlight == null)
            {
                flashlight = Instantiate(flashlightPrefab, transform.position, Quaternion.identity);
                flashlight.GetComponent<DelayedFollow>().targetX = transform.Find("Camera").gameObject;
                flashlight.GetComponent<DelayedFollow>().targetY = gameObject;
                flashlight.GetComponent<FlashlightFollowTarget>().target = transform.Find("FlashlightCorrectPos").gameObject;
                flashlight.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);
            }
            else
            {
                Destroy(flashlight);
            }
        }
    }
}
