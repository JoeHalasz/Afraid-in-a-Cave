using UnityEngine;

public class BobUpAndDown : MonoBehaviour
{
    
    [SerializeField]
    private float moveSpeed = .05f;
    private int direction = 1;
    [SerializeField]
    float maxHeight = .03f;
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void FixedUpdate() {
        if (transform.position.y > startPos.y + maxHeight)
            direction = -1;
        else if (transform.position.y < startPos.y - maxHeight)
            direction = 1;
        
        float distanceNormalized = Vector3.Distance(startPos, transform.position) / (maxHeight*2);

        float amountToAdd = (moveSpeed * direction * Time.deltaTime) / (Vector3.Distance(startPos, transform.position) + 1f);
        transform.position += new Vector3(0, amountToAdd, 0);
    }

}
