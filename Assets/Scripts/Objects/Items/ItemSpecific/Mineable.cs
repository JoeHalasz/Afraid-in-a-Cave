using UnityEngine;

public class Mineable : MonoBehaviour
{

    public void mine()
    {
        // make this not kinematic anymore, so it can fall, turn on the Item script and change the tag to "Pickup"
        Item item = GetComponent<Item>();
        if (item != null)
        {
            item.enabled = true;
        }
        else
        {
            Debug.LogError("No Item script found on: " + gameObject.name);
            return;
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = item.getMass();
        }
        else
        {
            Debug.LogError("No Rigidbody found on: " + gameObject.name);
            return;
        }

        item.gameObject.tag = "Pickup";
        item.gameObject.layer = LayerMask.NameToLayer("Item");
    }
    
}
