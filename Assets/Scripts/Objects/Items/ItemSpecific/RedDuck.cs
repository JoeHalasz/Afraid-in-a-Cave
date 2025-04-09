using UnityEngine;

public class RedDuck : MonoBehaviour
{
    
    Rigidbody rb;
    float bounceForce = 1.1f;

    Vector3 lastVelocity = Vector3.zero;
    bool bounced = false;
    Collision lastCollision = null;

    float timeUntilStart = 2f;

    float maxVelocity = 10000f;

    void Start()
    {
        Item item = GetComponent<Item>();
        item.changeMaxHealth(float.MaxValue);
        rb = GetComponent<Rigidbody>();
        // make it interpolate
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        if (timeUntilStart > 0f)
        {
            timeUntilStart -= Time.fixedDeltaTime;
            return;
        }
        if (bounced)
        {
            Debug.Log($"Old {lastVelocity}");
            Vector3 direction = Vector3.Reflect(lastVelocity, lastCollision.contacts[0].normal);
            if (direction.magnitude > maxVelocity)
                direction = direction.normalized * maxVelocity;
            Debug.Log($"New {direction*bounceForce}");
            rb.linearVelocity = direction * bounceForce;
            bounced = false;
        }
        lastVelocity = rb.linearVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        bounced = true;
        lastCollision = collision;
    }

}
