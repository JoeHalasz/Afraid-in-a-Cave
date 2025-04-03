using UnityEngine;

public class DelayedFollow : MonoBehaviour
{

    [SerializeField]
    private Transform target; // The target to follow
    [SerializeField]
    private float followSpeed = 7.5f;

    void Update()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime*followSpeed);
        }
    }
}
