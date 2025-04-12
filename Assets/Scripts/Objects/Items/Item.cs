using UnityEngine;
using Unity.Netcode;

public class Item : MonoBehaviour
{
    [SerializeField]
    float maxWorth = 100f;
    float damagePercent = 0f;
    [SerializeField]
    float mass = 100f;
    [SerializeField]
    float minDamageThreshold = 0.1f;
    private float maxHealth = 1f;
    public void changeMaxHealth(float amount) { maxHealth = amount; }

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

    void Awake()
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
        if (GetComponent<NetworkObject>() == null)
            Debug.LogError("Item does not have a NetworkObject component. Please add one.");
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
        gameObject.layer = LayerMask.NameToLayer("Item");
        // make sure it has a network object script component
    }

    // on collision with anything, make the damage go up based on how hard it hit the other object
    void OnCollisionEnter(Collision collision)
    {
        float collisionForce = collision.relativeVelocity.magnitude;
        float damage = Mathf.Clamp01(collisionForce / damageDampener);
        if (damage < minDamageThreshold) return;
        
        damagePercent += damage;
        if (damagePercent >= maxHealth)
        {
            damagePercent = maxHealth;
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
            audioSource.PlayOneShot(destroySound, 1f);
        gameObject.GetComponent<Renderer>().enabled = false;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Destroy(gameObject, 1f);
    }

}
