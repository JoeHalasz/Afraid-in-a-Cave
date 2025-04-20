using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;


public class ItemManager : MonoBehaviour
{

    // group items based on rarity
    public enum Rarity
    {
        Normal,
        Rare,
        Ultra
    }

    IDictionary<Rarity, List<GameObject>> itemsByRarity = new Dictionary<Rarity, List<GameObject>>();

    void Start()
    {
        itemsByRarity.Add(Rarity.Normal, new List<GameObject>());
        itemsByRarity.Add(Rarity.Rare, new List<GameObject>());
        itemsByRarity.Add(Rarity.Ultra, new List<GameObject>());

        // get every gameobject from the Resources/Prefabs/Items folder
        Object[] itemPrefabs = Resources.LoadAll("Prefabs/Items", typeof(GameObject));
        foreach (Object itemPrefab in itemPrefabs)
        {
            GameObject item = (GameObject)itemPrefab;
            Item itemScript = item.GetComponent<Item>();
            if (itemScript == null)
            {
                Debug.LogError("Item prefab does not have an Item script attached: " + item.name);
                continue;
            }
            // add the item to the list based on its rarity
            if (itemsByRarity.ContainsKey((Rarity)itemScript.getRarity()))
                itemsByRarity[(Rarity)itemScript.getRarity()].Add(item);
            else
                Debug.LogError("Item prefab has an invalid rarity: " + item.name);
        }
    }

    public void spawnItems(float maxValue)
    {
        Debug.Log("Spawning items...");
        float totalValue = 0f;
        
        int maxItems = 100;
        int numItems = 0;
        
        List<GameObject> spawners = new List<GameObject>(GameObject.FindGameObjectsWithTag("ItemSpawner"));
        while (totalValue < maxValue && numItems < maxItems)
        {
            if (spawners.Count == 0)
            {
                Debug.LogWarning("No spawners found. Cannot spawn items.");
                break;
            }

            // go through the spawners in a random order
            for (int i = 0; i < spawners.Count; i++)
            {
                int randomIndex = Random.Range(i, spawners.Count);
                GameObject temp = spawners[i];
                spawners[i] = spawners[randomIndex];
                spawners[randomIndex] = temp;
            }

            // loop through the spawners and spawn a random items based on the rarity. Normal is 83%, Rare is 15%, Ultra is 2%
            foreach (GameObject spawner in spawners)
            {
                float randomValue = Random.Range(0f, 1f);
                GameObject itemToSpawn = null;
                if (randomValue < 0.6f) // Normal
                    itemToSpawn = itemsByRarity[Rarity.Normal][Random.Range(0, itemsByRarity[Rarity.Normal].Count)];
                else if (randomValue < 0.95f) // Rare
                    itemToSpawn = itemsByRarity[Rarity.Rare][Random.Range(0, itemsByRarity[Rarity.Rare].Count)];
                else // Ultra
                    itemToSpawn = itemsByRarity[Rarity.Ultra][Random.Range(0, itemsByRarity[Rarity.Ultra].Count)];

                ItemSpawner itemSpawner = spawner.GetComponent<ItemSpawner>();
                totalValue += itemToSpawn.GetComponent<Item>().getMaxWorth();
                numItems++;
                // Debug.Log("Spawning item: " + itemToSpawn.name + " with value: " + itemToSpawn.GetComponent<Item>().getMaxWorth());
                itemSpawner.SpawnItem(itemToSpawn);
                if (totalValue >= maxValue || numItems >= maxItems)
                    break;
            }
        }
        Debug.Log("Spawned " + numItems + " items with a total value of " + totalValue + " worth of items.");
    }
}
