using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

public class SyncMass : NetworkBehaviour
{

    public NetworkVariable<float> mass = new NetworkVariable<float>(0);

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // if we are the host and we are the owner
        if (!HasAuthority || !IsSpawned) return;
        mass.Value = rb.mass;
    }

    // add listener to mass variable
    public void OnEnable()
    {
        mass.OnValueChanged += OnMassChanged;
    }

    void OnMassChanged(float oldValue, float newValue)
    {
        Debug.Log($"Mass of {gameObject.name} changed from {oldValue} to {newValue}");
        if (!HasAuthority && rb != null)
            rb.mass = newValue;
    }

}
