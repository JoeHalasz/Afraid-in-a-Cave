using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    int maxHealth = 100;
    int health = 100;
    public int getHealth() { return health; }

    int maxInsanity = 100;
    int insanity = 0;
    public int getInsanity() { return insanity; }

    Player player;

    void Start()
    {
        player = GetComponent<Player>();
        if (player == null) Debug.LogError("PlayerStats: Player component not found on " + gameObject.name);
    }

    public void addHealth(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        if (health <= 0)
        {
            health = 0;
            player.die();
        }
    }

    public void addInsanity(int amount)
    {
        insanity += amount;
        if (insanity >= maxInsanity)
        {
            insanity = maxInsanity;
            player.die();
        }
        if (insanity < 0) insanity = 0;
    }
}
