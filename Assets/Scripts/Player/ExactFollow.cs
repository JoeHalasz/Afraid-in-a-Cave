using UnityEngine;

public class ExactFollow : MonoBehaviour
{

    [SerializeField]
    public GameObject target;

    void FixedUpdate()
    {
        if (target == null) return;
        transform.position = target.transform.position;
        transform.rotation = target.transform.rotation;
    }

}
