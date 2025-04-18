using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Item : NetworkBehaviour
{
    [SerializeField]
    float maxWorth = 100f;
 
    public NetworkVariable<float> damagePercent = new NetworkVariable<float>(0);
 
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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = true;
        // add rocks, player, and item as include layers
        rb.includeLayers = LayerMask.GetMask("Rock", "Player", "Item");
        MeshCollider c = GetComponent<MeshCollider>();
        if (c == null && GetComponent<Collider>() == null)
            c = gameObject.AddComponent<MeshCollider>();
        if (c != null)
            c.convex = true;
        if (GetComponent<NetworkObject>() == null)
            Debug.LogError("Item does not have a NetworkObject component. Please add one.");
        if (GetComponent<NetworkTransform>() == null)
            Debug.LogError("Item does not have a NetworkTransform component. Please add one.");
        if (GetComponent<NetworkRigidbody>() == null)
            Debug.LogError("Item does not have a NetworkRigidbody component. Please add one.");
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.maxDistance = 10f; // max distance for the sound to be heard
        audioSource = GetComponent<AudioSource>();
        // set tag to Pickup
        gameObject.tag = "Pickup";
        // set layer to Rock
        gameObject.layer = LayerMask.NameToLayer("Item");
        // make sure it has a network object script component
    }

    void Start()
    {
        // get the item config from LoadItemConfig.Instance
        string itemName = gameObject.name;
        itemName = itemName.Replace("(Clone)", "");
        if (LoadItemConfig.Instance.itemConfigs.ContainsKey(itemName))
        {
            LoadItemConfig.ItemConfig config = LoadItemConfig.Instance.itemConfigs[itemName];
            maxWorth = config.maxWorth;
            mass = config.mass;
            minDamageThreshold = config.minDamageThreshold;
            damageDampener = config.damageDampener;
            rarity = (Rarity)System.Enum.Parse(typeof(Rarity), config.rarity);
        }
        else
        {
            Debug.LogError($"Item {itemName} not found in item config. Please add to Assets/Scripts/Objects/ItemConfig.txt.");
        }
    }

    // on collision with anything, make the damage go up based on how hard it hit the other object
    void OnCollisionEnter(Collision collision)
    {
        if (!HasAuthority || !IsSpawned) return;
        float collisionForce = collision.relativeVelocity.magnitude;
        float damage = Mathf.Clamp01(collisionForce / damageDampener);
        if (damage < minDamageThreshold) return;
        
        damagePercent.Value += damage;
        if (damagePercent.Value >= maxHealth)
        {
            damagePercent.Value = maxHealth;
            itemBreak();
            return;
        }
        worth = maxWorth * (1f - damagePercent.Value);

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
        Debug.Log($"Item {gameObject.name} broke");
        if (destroySound != null)
            audioSource.PlayOneShot(destroySound, 1f);
        gameObject.GetComponent<Renderer>().enabled = false;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Destroy(gameObject, 1f);
    }

}
