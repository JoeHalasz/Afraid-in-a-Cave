using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerChange : NetworkBehaviour
{

    List<string> bodyTypes = new List<string>() { "NormalPlayer", "Slender" };
    public List<string> getBodyTypes() { return bodyTypes; }
    string currentBodyType = "NormalPlayer";
    public string getCurrentBodyType() { return currentBodyType; }
    SyncVars syncVars;

    void Start()
    {
        syncVars = GetComponent<SyncVars>();

        foreach (string bodyType in bodyTypes)
        {
            if (bodyType == "NormalPlayer") continue;
            GameObject body = transform.Find(bodyType).gameObject;
            if (body != null)
                body.SetActive(false);
        }
        // activate NormalPlayer
        activateBody(0);
    }

    void activateBody(int bodyIndex)
    {
        if (bodyIndex < 0 || bodyIndex >= bodyTypes.Count) return;
        GameObject newBody = transform.Find(bodyTypes[bodyIndex])?.gameObject;
        if (newBody != null)
        {
            if (HasAuthority)
                syncVars.playerBody.Value = bodyIndex;
            currentBodyType = bodyTypes[bodyIndex];
            newBody.SetActive(true);
        }
    }

    // if the player presses the "1" key, change to first bodyType
    // if the player presses the "2" key, change to second bodyType
    // do for all number keys
    // change by setting it to active and deactivating the others and changing currentBodyType

    void changeToBody(int bodyIndex)
    {
        if (bodyTypes[bodyIndex] == currentBodyType) return;
        GameObject currentBody = transform.Find(currentBodyType).gameObject;
        if (currentBody != null)
            currentBody.SetActive(false);
        activateBody(bodyIndex);
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        for (int i = 0; i < bodyTypes.Count; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                changeToBody(i);
    }

    void FixedUpdate()
    {
        int newBody = syncVars.playerBody.Value;
        if (bodyTypes[newBody] != currentBodyType)
            changeToBody(newBody);
    }

}
