using UnityEngine;
using Unity.Netcode;

public class PickupItem : NetworkBehaviour
{
    
    [SerializeField]
    GameObject pickedUpItem;
    Rigidbody pickedUpItemRB;
    GameObject cam;

    [SerializeField]
    float holdDistance = 1f; // distance to hold the item at
    float breakDistance = 3f;

    float moveForce = 500f;
    [SerializeField]
    float range = 4f;

    LayerMask layerMask;

    void Start()
    {
        // get the camera child object
        cam = transform.Find("Camera").gameObject;
        layerMask = LayerMask.GetMask("Item");
    }

    public void checkPickupItem()
    {
        // raycast to check if the player is looking at the item
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, layerMask))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject != gameObject)
                if (hit.collider.gameObject.tag == "Pickup")
                    pickupItem(hit.collider.gameObject);
        }
        // draw the ray
        Debug.DrawRay(cam.transform.position, cam.transform.forward * range, Color.red);
    }

    private void pickupItem(GameObject item)
    {
        pickedUpItem = item;
        pickedUpItemRB = item.GetComponent<Rigidbody>();
        if (pickedUpItemRB != null)
            pickedUpItemRB.useGravity = false;
        transferOwnershipOnPickup(item);
    }

    private void dropItem()
    {
        if (pickedUpItemRB != null)
            pickedUpItemRB.useGravity = true;
        pickedUpItem = null;
        pickedUpItemRB = null;
    }

    public void FixedUpdate()
    {
        if (!HasAuthority || !IsSpawned) return;
        if (Input.GetMouseButton(0) && pickedUpItem == null)
            checkPickupItem();
        // while the mouse is held down, make the item float towards infront of the player by the holdDistance
        if (Input.GetMouseButton(0) && pickedUpItem != null)
        {
            Vector3 targetPosition = cam.transform.position + cam.transform.forward * holdDistance;
            // accelerate the RB based on the items mass, less acceleration for heavier items
            float acceleration = moveForce / pickedUpItemRB.mass;
            // move the item towards the target position
            pickedUpItemRB.linearVelocity = Vector3.Lerp(pickedUpItemRB.linearVelocity, (targetPosition - pickedUpItem.transform.position) * acceleration, Time.fixedDeltaTime * 5f);
            // if its too close to the player camera then push it away 
            if (Vector3.Distance(cam.transform.position, pickedUpItem.transform.position) < 1f)
                pickedUpItemRB.linearVelocity += cam.transform.forward * acceleration * Time.deltaTime * 3f;
            pickedUpItemRB.angularVelocity = Vector3.Lerp(pickedUpItemRB.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
        }
        // if the item gets too far away then drop the item
        if (pickedUpItem != null && Vector3.Distance(cam.transform.position, pickedUpItem.transform.position) > breakDistance)
            dropItem();
        // if the mouse is released then drop the item
        if (!Input.GetMouseButton(0) && pickedUpItem != null)
            dropItem();
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
                itemNetworkObject.ChangeOwnership(NetworkManager.LocalClientId);
                Debug.Log($"Ownership of {item.name} transferred to {NetworkManager.LocalClientId}");
            }
        }
    }
}
