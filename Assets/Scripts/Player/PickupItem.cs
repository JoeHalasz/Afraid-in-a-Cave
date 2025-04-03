using UnityEngine;
using Unity.Netcode;

public class PickupItem : NetworkBehaviour
{
    
    GameObject pickedUpItem;
    Rigidbody pickedUpItemRB;
    GameObject camera;

    float holdDistance = 2f; // distance to hold the item at
    float breakDistance = 3f;

    void Start()
    {
        // get the camera child object
        camera = GameObject.Find("Camera");
    }

    public void checkPickupItem()
    {
        // raycast to check if the player is looking at the item
        RaycastHit hit;
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 2f))
        {
            if (hit.collider.gameObject != gameObject)
            {
                if (hit.collider.gameObject.tag == "Pickup")
                    pickupItem(hit.collider.gameObject);
            }
        }
        // draw the ray
        Debug.DrawRay(camera.transform.position, camera.transform.forward * 2f, Color.red);
    }

    private void pickupItem(GameObject item)
    {
        pickedUpItem = item;
        pickedUpItemRB = item.GetComponent<Rigidbody>();
        if (pickedUpItemRB != null)
        {
            pickedUpItemRB.useGravity = false;
        }
        transferOwnershipOnPickup(item);
    }

    private void dropItem()
    {
        if (pickedUpItemRB != null)
        {
            pickedUpItemRB.useGravity = true;
        }
        pickedUpItem = null;
        pickedUpItemRB = null;
    }

    public void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && pickedUpItem == null)
        {
            checkPickupItem();
        }
        // while the mouse is held down, make the item float towards infront of the player by the holdDistance
        if (Input.GetMouseButton(0) && pickedUpItem != null)
        {
            Vector3 targetPosition = camera.transform.position + camera.transform.forward * holdDistance;
            // accelerate the RB based on the items mass, less acceleration for heavier items
            float acceleration = 2f / pickedUpItemRB.mass;
            pickedUpItemRB.linearVelocity = Vector3.Lerp(pickedUpItemRB.linearVelocity, (targetPosition - pickedUpItem.transform.position) * acceleration, Time.deltaTime * 5f);
            // TODO slow the items rotation to 0
            pickedUpItemRB.angularVelocity = Vector3.Lerp(pickedUpItemRB.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
        }
        // if the item gets too far away then drop the item
        if (pickedUpItem != null && Vector3.Distance(camera.transform.position, pickedUpItem.transform.position) > breakDistance)
        {
            dropItem();
        }
        // if the mouse is released then drop the item
        if (!Input.GetMouseButton(0) && pickedUpItem != null)
        {
            dropItem();
        }
    }

    private void transferOwnershipOnPickup(GameObject item)
    {
        if (!HasAuthority || !IsSpawned) return;

        if (item == null) return;

        // Transfer ownership of the item to the player who picked it up
        NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
        if (itemNetworkObject != null && itemNetworkObject.IsSpawned)
        {
            // if this isnt already the owner
            if (!itemNetworkObject.IsOwner)
            {
                itemNetworkObject.ChangeOwnership(OwnerClientId);
                Debug.Log($"Ownership of {item.name} transferred to {OwnerClientId}");
            }
        }
    }
}
