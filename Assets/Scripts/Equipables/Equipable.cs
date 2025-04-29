using UnityEngine;
using Unity.Netcode;

public class Equipable : NetworkBehaviour
{

    void Start()
    {
        OnEquipt();
    }

    public void OnEquipt()
    {
        if (!HasAuthority || !IsSpawned) return;
        transform.localPosition = new Vector3(.6f, -.6f, .85f);
        Debug.Log("Set local position to: " + transform.localPosition);
        MonoBehaviour script = GetComponent(gameObject.name.Replace("(Clone)","")) as MonoBehaviour;
        if (script != null)
            script.enabled = true;
        else
            Debug.LogError("No script found with the name: " + gameObject.name.Replace("(Clone)",""));

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = true;
        else
            Debug.LogError("No MeshRenderer found on: " + gameObject.name);
    }

    public void OnUnequipt()
    {
        if (!HasAuthority || !IsSpawned) return;
        MonoBehaviour script = GetComponent(gameObject.name.Replace("(Clone)","")) as MonoBehaviour;
        if (script != null)
            script.enabled = false;
        else
            Debug.LogError("No script found with the name: " + gameObject.name.Replace("(Clone)",""));
    
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
        else
            Debug.LogError("No MeshRenderer found on: " + gameObject.name);
    }
}
