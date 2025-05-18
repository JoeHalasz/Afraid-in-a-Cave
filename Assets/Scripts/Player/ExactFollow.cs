using UnityEngine;

public class ExactFollow : MonoBehaviour
{
    [SerializeField]
    public GameObject target;

    public Vector3 positionOffset = new Vector3(0, 0, 0);
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public bool followRotation = true;


    void FixedUpdate()
    {
        if (target == null) return;
        transform.position = target.transform.position + positionOffset;
        if (followRotation)
            transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles + rotationOffset);
    }

}
