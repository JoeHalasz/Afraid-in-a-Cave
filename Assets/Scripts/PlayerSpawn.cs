using UnityEngine;
using Unity.Netcode;

public class PlayerSpawn : NetworkBehaviour
{

    [SerializeField]
    GameObject camera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            camera.SetActive(true);
        }
    }
}
