using UnityEngine;
using Unity.Netcode;

public class DelayedFollow : NetworkBehaviour
{
    [SerializeField]
    public GameObject targetX; // The target to follow for X-axis rotation
    [SerializeField]
    public GameObject targetY; // The target to follow for Y-axis rotation
    [SerializeField]
    private float followSpeed = 7.5f;

    bool wasNullX = true;
    bool wasNullY = true;

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;


        Vector3 currentEulerAngles = transform.localEulerAngles;

        if (wasNullX && targetX != null)
        {
            wasNullX = false;
            Vector3 targetXRotation = targetX.transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(targetXRotation.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
        else if (targetX != null)
        {
            float targetXRotation = targetX.transform.localEulerAngles.x;
            currentEulerAngles.x = Mathf.LerpAngle(currentEulerAngles.x, targetXRotation, Time.deltaTime * followSpeed);
        }
        
        if (!wasNullY && targetY == null)
        {
            wasNullY = true;
            Vector3 targetYRotation = targetY.transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, targetYRotation.y, transform.localEulerAngles.z);
        }
        else if (targetY != null)
        {
            float targetYRotation = targetY.transform.localEulerAngles.y;
            currentEulerAngles.y = Mathf.LerpAngle(currentEulerAngles.y, targetYRotation, Time.deltaTime * followSpeed);
        }

        // Apply the updated rotation
        transform.localEulerAngles = currentEulerAngles;
    }
}
