using UnityEngine;
using Unity.Netcode;

public class RotationLock : NetworkBehaviour
{

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
