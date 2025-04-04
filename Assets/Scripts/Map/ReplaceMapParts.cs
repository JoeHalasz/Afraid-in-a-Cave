using UnityEngine;
using System.Collections.Generic;


public class ReplaceMapParts : MonoBehaviour
{

    // dictionary of names to prefabs
    IDictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    GameObject mapManager = null;

    void Start()
    {
        mapManager = GameObject.Find("MapManager");
        foreach (GameObject obj in Resources.LoadAll<GameObject>("Prefabs/MapPartsFinished"))
        {
            if (obj != null)
            {
                prefabs.Add(obj.name, obj);
            }
        }
    }

    public void replaceParts()
    {
        List<GameObject> newParts = new List<GameObject>();
        // for every child in map manager
        foreach (Transform child in mapManager.transform)
        {
            if (child.GetComponent<SavedPartType>() == null)
            {
                Debug.LogError($"Child {child.name} does not have a SavedPartType component.");
                continue;
            }
            string name = child.GetComponent<SavedPartType>().partTypeName;
            // if the name is in the dictionary, replace the child with the prefab
            if (prefabs.ContainsKey(name))
            {
                GameObject prefab = prefabs[name];
                GameObject newPart = Instantiate(prefab, child.position, child.rotation);
                // add a random very small y position to prevent y clipping
                newPart.transform.position += new Vector3(0, Random.Range(-0.001f, 0.001f), 0);
                newParts.Add(newPart);                
                // find all the connection points in the child
                ConnectConnectionPoints[] connectionPoints = child.GetComponentsInChildren<ConnectConnectionPoints>();
                // if its connected to anything then delete the connection point in the new part
                foreach (ConnectConnectionPoints connectionPoint in connectionPoints)
                {
                    if (connectionPoint.connection != null)
                    {
                        // find the same named child in the new part
                        Transform newPartConnectionPoint = newPart.transform.Find(connectionPoint.name);
                        if (newPartConnectionPoint != null)
                        {
                            Destroy(newPartConnectionPoint.gameObject);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find connection point {connectionPoint.name} in new part {newPart.name}.");
                        }
                    }
                }

                // destroy the old part
                Destroy(child.gameObject);
            }
        }

        foreach (GameObject newPart in newParts)
        {
            newPart.transform.parent = mapManager.transform;
        }
    }

}
