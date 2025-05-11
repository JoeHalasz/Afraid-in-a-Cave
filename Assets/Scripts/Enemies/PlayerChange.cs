using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerChange : NetworkBehaviour
{

    List<string> bodyTypes = new List<string>() { "NormalPlayer", "Slender" };
    string currentBodyType = "NormalPlayer";

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        // deactivate all bodyType on this object
        foreach (string bodyType in bodyTypes)
        {
            GameObject body = transform.Find(bodyType).gameObject;
            if (body != null)
                body.SetActive(false);
        }
        // activate NormalPlayer
        GameObject normalBody = transform.Find("NormalPlayer").gameObject;
        if (normalBody != null)
            normalBody.SetActive(true);
    }

    // if the player presses the "1" key, change to first bodyType
    // if the player presses the "2" key, change to second bodyType
    // do for all number keys
    // change by setting it to active and deactivating the others and changing currentBodyType

    void changeToBody(int bodyIndex)
    {
        Debug.Log("Changing to body1: " + bodyTypes[bodyIndex]);
        if (bodyTypes[bodyIndex] == currentBodyType) return;
        Debug.Log("Changing to body2: " + bodyTypes[bodyIndex]);
        GameObject currentBody = transform.Find(currentBodyType).gameObject;
        if (currentBody != null)
            currentBody.SetActive(false);
        GameObject newBody = transform.Find(bodyTypes[bodyIndex]).gameObject;
        if (newBody != null)
            newBody.SetActive(true);
        currentBodyType = bodyTypes[bodyIndex];
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        for (int i = 0; i < bodyTypes.Count; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                changeToBody(i);
    }

}
