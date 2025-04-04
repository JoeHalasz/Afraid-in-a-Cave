using UnityEngine;

public class DelayedFollow : MonoBehaviour
{

    [SerializeField]
    private Transform target; // The target to follow
    [SerializeField]
    private float followSpeed = 7.5f;
    GameObject correctPosTarget = null;
    
    void Start()
    {
        correctPosTarget = GameObject.Find("FlashlightCorrectPos");
    }

    void Update()
    {
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
