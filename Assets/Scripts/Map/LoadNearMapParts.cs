using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class LoadNearMapParts : NetworkBehaviour
{
    
    public List<GameObject> closeMapParts = new List<GameObject>();
    public List<GameObject> loadedFrom = new List<GameObject>(); // list of game objects that this is loaded because of 
    public GameObject replacementObject = null;

    public void addLoadedFrom(GameObject obj)
    {
        loadedFrom.Add(obj);
        checkShouldBeLoaded();
    }
    public void removeLoadedFrom(GameObject obj)
    {
        loadedFrom.Remove(obj);
        Invoke("checkShouldBeLoaded", 1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // if its the player
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.GetComponent<NetworkObject>().IsOwner)
            {
                foreach (GameObject obj in closeMapParts)
                {
                    LoadNearMapParts otherScript = obj.GetComponentInChildren<LoadNearMapParts>();
                    if (otherScript == null)
                    {
                        Debug.LogWarning($"LoadNearMapParts script not found on {obj.name}.");
                        continue;
                    }
                    if (!otherScript.loadedFrom.Contains(gameObject))
                        otherScript.addLoadedFrom(gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // if its the player
        if (other.gameObject.CompareTag("Player"))
        {
            // make sure that player has authority
            if (other.gameObject.GetComponent<NetworkObject>().IsOwner)
            {
                foreach (GameObject obj in closeMapParts)
                {
                    LoadNearMapParts otherScript = obj.GetComponentInChildren<LoadNearMapParts>();
                    if (otherScript == null)
                    {
                        Debug.LogWarning($"LoadNearMapParts script not found on {obj.name}.");
                        continue;
                    }
                    if (otherScript.loadedFrom.Contains(gameObject))
                        otherScript.removeLoadedFrom(gameObject);
                }
            }
        }
    }

    public void checkShouldBeLoaded()
    {
        if (loadedFrom.Count == 0)
        {
            unload();
        }
        else
        {
            load();
        }
    }

    void recursiveUnload(GameObject obj)
    {
        if (obj.name.Contains("Rock") || obj.name.Contains("Lantern"))
            obj.SetActive(false);
        else if (!obj.name.Contains("ItemBench"))
            foreach (Transform child in obj.transform)
                recursiveUnload(child.gameObject);
    }

    void recursiveLoad(GameObject obj)
    {
        if (obj.name.Contains("Rock") || obj.name.Contains("Lantern"))
            obj.SetActive(true);
        else if (!obj.name.Contains("ItemBench"))
            foreach (Transform child in obj.transform)
                recursiveLoad(child.gameObject);
    }

    void unload()
    {
        recursiveUnload(transform.parent.gameObject);
    }

    void load()
    {
        recursiveLoad(transform.parent.gameObject);
    }

}
