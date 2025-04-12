using System.Threading.Tasks;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    
    void Start()
    {
        gameObject.tag = "ItemSpawner";
    }

    public async Task SpawnItem(GameObject itemPrefab)
    {
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
        
        float scaleChange = Random.Range(-0.6f, 0.3f);
        item.transform.localScale = new Vector3(
            item.transform.localScale.x + item.transform.localScale.x * scaleChange,
            item.transform.localScale.y + item.transform.localScale.y * scaleChange,
            item.transform.localScale.z + item.transform.localScale.z * scaleChange
        );
        
        // set the item to be a child of this object
        item.transform.parent = transform.parent;
    }

}
