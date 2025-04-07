using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    float maxWorth = 100f;
    float damagePercent = 0f;
    [SerializeField]
    float mass = 100f;
    [SerializeField]
    float minDamageThreshold = 0.1f;

    [SerializeField]
    float damageDampener = 25f;

    [SerializeField]
    AudioClip hurtSound;
    [SerializeField]
    AudioClip destroySound;
    AudioSource audioSource;

    public enum Rarity
    {
        Normal,
        Rare,
        Ultra
    }

    [SerializeField]
    Rarity rarity = Rarity.Normal;

    public Rarity getRarity() { return rarity; }

    // make it so you can see the worth but not edit it in the inspector
    [SerializeField]
    float worth = 0f;
    public float getWorth() { return worth; }
    public float getMaxWorth() { return maxWorth; }

    void Start()
    {
        worth = maxWorth;
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.useGravity = true;
            // add rocks, player, and item as include layers
            rb.includeLayers = LayerMask.GetMask("Rock", "Player", "Item");
        }
        if (GetComponent<Collider>() == null)
        {
            MeshCollider c = gameObject.AddComponent<MeshCollider>();
            c.convex = true;
        }
        if (GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.maxDistance = 10f; // max distance for the sound to be heard
        }
        audioSource = GetComponent<AudioSource>();
        // set tag to Pickup
        gameObject.tag = "Pickup";
        // set layer to Rock
        gameObject.layer = LayerMask.NameToLayer("Rock");
    }

    // on collision with anything, make the damage go up based on how hard it hit the other object
    void OnCollisionEnter(Collision collision)
    {
        float collisionForce = collision.relativeVelocity.magnitude;
        Debug.Log($"Collision force: {collisionForce}");
        float damage = Mathf.Clamp01(collisionForce / damageDampener);
        if (damage < minDamageThreshold) return;
        
        damagePercent += damage;
        if (damagePercent >= 1f)
        {
            damagePercent = 1f;
            itemBreak();
            return;
        }
        worth = maxWorth * (1f - damagePercent);

        makeRed();
        if (hurtSound != null)
            audioSource.PlayOneShot(hurtSound, 1f);
    }

    Color originalColor;

    void makeRed()
    {
        // make the item flash red for a second
        if (originalColor == null || GetComponent<Renderer>().material.color != Color.red)
            originalColor = GetComponent<Renderer>().material.color;
        GetComponent<Renderer>().material.color = Color.red;
        Invoke("makeNormal", .2f);
    }

    void makeNormal()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }

    public void itemBreak()
    {
        if (destroySound != null)
        {
            audioSource.PlayOneShot(destroySound, 1f);
        }
        gameObject.GetComponent<Renderer>().enabled = false;
        Destroy(gameObject, 1f);
    }

}
