using UnityEngine;

public class RotationLock : MonoBehaviour
{

    
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
