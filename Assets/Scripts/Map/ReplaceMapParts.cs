using UnityEngine;
using System.Collections.Generic;


public class ReplaceMapParts : MonoBehaviour
{

    // dictionary of names to prefabs
    IDictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    GameObject mapManager = null;
    IDictionary<GameObject, GameObject> replacements = new Dictionary<GameObject, GameObject>();

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
                replacements.Add(child.gameObject, newPart);
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
                // get the LoadNearMapParts component from newParts child called LoadNearMapParts
                LoadNearMapParts loadNearMapParts = newPart.GetComponentInChildren<LoadNearMapParts>();
                loadNearMapParts.replacementObject = child.gameObject;
            }
        }

        foreach (GameObject newPart in newParts)
        {
            newPart.transform.parent = mapManager.transform;
            LoadNearMapParts loadNearMapParts = newPart.GetComponentInChildren<LoadNearMapParts>();
            GameObject oldPart = loadNearMapParts.replacementObject;
            List<GameObject> closeParts = findCloseParts(oldPart);
            loadNearMapParts.closeMapParts = closeParts;
        }
    }

    List<GameObject> findCloseParts(GameObject obj) // this should have connectionPoints as children
    {
        // Debug.Log($"Finding close parts for {obj.name}");
        List<GameObject> closeParts = new List<GameObject>();
        List<GameObject> secondLayer = new List<GameObject>();
        ConnectConnectionPoints[] connectionPoints = obj.GetComponentsInChildren<ConnectConnectionPoints>();
        // if its connected to anything then delete the connection point in the new part
        closeParts.Add(replacements[obj]);
        foreach (ConnectConnectionPoints connectionPoint in connectionPoints)
        {
            if (connectionPoint.connection != null)
            {
                GameObject connectionParent = connectionPoint.connection.transform.parent.gameObject;
                secondLayer.Add(connectionParent);
                if (replacements.ContainsKey(connectionParent) && !closeParts.Contains(replacements[connectionParent]))
                    closeParts.Add(replacements[connectionParent]);
            }
        }
        foreach (GameObject connectionParent in secondLayer)
        {
            ConnectConnectionPoints[] connectionPoints2 = connectionParent.GetComponentsInChildren<ConnectConnectionPoints>();
            // if its connected to anything then delete the connection point in the new part
            foreach (ConnectConnectionPoints connectionPoint in connectionPoints2)
            {
                if (connectionPoint.connection != null)
                {
                    GameObject connectionParent2 = connectionPoint.connection.transform.parent.gameObject;
                    if (replacements.ContainsKey(connectionParent2) && !closeParts.Contains(replacements[connectionParent2]))
                        closeParts.Add(replacements[connectionParent2]);
                }
            }
        }
        // Debug.Log($"Found {closeParts.Count} close parts for {obj.name}");
        return closeParts;
    }

}
