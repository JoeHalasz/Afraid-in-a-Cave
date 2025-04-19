using UnityEngine;
using Unity.Netcode;

public class RedDuck : NetworkBehaviour
{
    Rigidbody rb;
    float bounceForce = 1.1f;

    Vector3 lastVelocity = Vector3.zero;
    bool bounced = false;
    Collision lastCollision = null;

    float timeUntilStart = 2f;

    float maxVelocity = 10000f;

    [SerializeField]
    AudioClip bounceSound = null;

    void Start()
    {
        Item item = GetComponent<Item>();
        item.changeMaxHealth(float.MaxValue);
        rb = GetComponent<Rigidbody>();
        // Make it interpolate
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
            Vector3 direction = Vector3.Reflect(lastVelocity, lastCollision.contacts[0].normal);
            if (direction.magnitude > maxVelocity)
                direction = direction.normalized * maxVelocity;
            rb.linearVelocity = direction * bounceForce;
            bounced = false;
        }
        lastVelocity = rb.linearVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        bounced = true;
        lastCollision = collision;

        float soundVolume = Mathf.Clamp01(rb.linearVelocity.magnitude / maxVelocity);
        soundVolume = Mathf.Clamp(soundVolume, 0.1f, 1f);

        // Notify the server about the bounce
        PlayBounceSoundServerRpc(soundVolume);
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayBounceSoundServerRpc(float volume)
    {
        // Broadcast to all clients to play the sound
        PlayBounceSoundClientRpc(volume);
    }

    [ClientRpc]
    void PlayBounceSoundClientRpc(float volume)
    {
        if (bounceSound != null)
        {
            AudioSource.PlayClipAtPoint(bounceSound, transform.position, volume);
        }
    }
}
