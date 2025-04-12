using UnityEngine;
using Unity.Netcode;

public class DelayedFollow : NetworkBehaviour
{

    [SerializeField]
    private Transform target; // The target to follow
    [SerializeField]
    private float followSpeed = 7.5f;
    [SerializeField]
    GameObject correctPosTarget = null;
    
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        if (target != null)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime*followSpeed);
        }
        if (correctPosTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, correctPosTarget.transform.position, Time.deltaTime*followSpeed);
        }
    }
}
