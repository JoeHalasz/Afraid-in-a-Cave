using UnityEngine;

public class Equipable : MonoBehaviour
{

    void Start()
    {
        OnEquip();
    }

    public void OnEquip()
    {
        transform.localPosition = new Vector3(.6f, -.6f, .85f);
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

    public void OnUnequip()
    {
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
