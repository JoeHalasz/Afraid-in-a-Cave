using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using Unity.Netcode;
public class ItemSpawner : MonoBehaviour
{
    
    void Start()
    {
        gameObject.tag = "ItemSpawner";
    }

    public void SpawnItem(GameObject itemPrefab)
    {
        // if the item is not spawned yet call this function in 1 second 

        // spawn an item randomly inside of this objects bounds, accounting for this items rotation
        // get the bounds of this object
        Bounds bounds = GetComponent<Collider>().bounds;
        bounds.min -= transform.position;
        bounds.max -= transform.position;

        Vector3 spawnPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            (bounds.min.y - bounds.max.y) / 2f,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        spawnPosition += transform.position;

        Vector3 spawnRotation = new Vector3( 0, Random.Range(0, 360), 0 );

        GameObject item = Instantiate(itemPrefab, spawnPosition, Quaternion.Euler(spawnRotation));
        item.GetComponent<NetworkObject>().Spawn(true);
        
        float scaleChange = Random.Range(-0.6f, 0.3f);
        item.transform.localScale = new Vector3(
            item.transform.localScale.x + item.transform.localScale.x * scaleChange,
            item.transform.localScale.y + item.transform.localScale.y * scaleChange,
            item.transform.localScale.z + item.transform.localScale.z * scaleChange
        );
        
        // StartCoroutine(parentNetworkObject(item));
    }

    // coroutine like parentNetworkObject function
    IEnumerator parentNetworkObject(GameObject item)
    {
        Debug.Log($"Parenting item {item.name} to {transform.parent.name}");
        if (!item.GetComponent<NetworkObject>().TrySetParent(transform.parent))
        {
            Debug.Log("trying again");
            yield return new WaitForSeconds(.5f);
            StartCoroutine(parentNetworkObject(item));
        }
    }
}
