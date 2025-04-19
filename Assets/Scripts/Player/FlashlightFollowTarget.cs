using UnityEngine;

public class FlashlightFollowTarget : MonoBehaviour
{
    
    public GameObject target;
    
    void Update()
    {
        if (target != null)
            transform.position = target.transform.position;
    }
}
