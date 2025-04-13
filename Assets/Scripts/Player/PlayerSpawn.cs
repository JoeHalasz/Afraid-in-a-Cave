using UnityEngine;
using Unity.Netcode;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField]
    GameObject camera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner) // Ensure this is the local player's object
        {
            Debug.Log("PlayerSpawn: OnNetworkSpawn - Local player camera enabled.");
            camera.SetActive(true);

            // Disable cameras on other players
            foreach (var player in Object.FindObjectsByType<PlayerSpawn>(FindObjectsSortMode.None))
            {
                if (player != this)
                {
                    player.camera.SetActive(false);
                }
            }
        }
        else
        {
            camera.SetActive(false); // Disable the camera for non-local players
        }
    }
}
