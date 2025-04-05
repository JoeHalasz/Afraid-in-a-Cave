using UnityEngine;
using System.Collections.Generic;

public class LoadNearMapParts : MonoBehaviour
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
            foreach (GameObject obj in closeMapParts)
            {
                LoadNearMapParts otherScript = obj.GetComponentInChildren<LoadNearMapParts>();
                if (otherScript == null)
                {
                    Debug.LogWarning($"LoadNearMapParts script not found on {obj.name}.");
                    continue;
                }
                if (!otherScript.loadedFrom.Contains(gameObject))
                {
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
            foreach (GameObject obj in closeMapParts)
            {
                LoadNearMapParts otherScript = obj.GetComponentInChildren<LoadNearMapParts>();
                if (otherScript == null)
                {
                    Debug.LogWarning($"LoadNearMapParts script not found on {obj.name}.");
                    continue;
                }
                if (otherScript.loadedFrom.Contains(gameObject))
                {
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

    void unload()
    {
        replacementObject.SetActive(true);
        // disable every object on this objects parent except this object
        foreach (Transform child in transform.parent)
        {
            if (child.gameObject != gameObject)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void load()
    {
        replacementObject.SetActive(false);
        // enable every object on this objects parent except this object
        foreach (Transform child in transform.parent)
        {
            if (child.gameObject != gameObject)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

}
