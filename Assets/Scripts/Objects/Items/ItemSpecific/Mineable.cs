using UnityEngine;
using Unity.Netcode;

public class Mineable : NetworkBehaviour
{

    public void mine()
    {
        Item item = GetComponent<Item>();
        if (item != null)
        {
            item.enabled = true;
            item.gameObject.tag = "Pickup";
            item.gameObject.layer = LayerMask.NameToLayer("Item");
            item.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            item.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        else
        {
            // its the big crystal, move its child and destroy it 
            foreach (Transform child in transform)
            {
                child.gameObject.GetComponent<Item>().setInvincibilityTime(1f);
                child.gameObject.tag = "Pickup";
                child.gameObject.layer = LayerMask.NameToLayer("Item");
                child.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                child.GetComponent<Rigidbody>().isKinematic = false;
                child.SetParent(null);
                child.gameObject.GetComponent<NetworkObject>().Spawn();
            }
            Destroy(gameObject);
        }
        

    }
    
}
