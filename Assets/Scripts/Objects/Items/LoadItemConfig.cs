using UnityEngine;
using System.Collections.Generic;

public class LoadItemConfig : MonoBehaviour
{

    public static LoadItemConfig Instance { get; private set; }
    void Awake() 
    {
        Instance = this;
        loadConfig(configFile);
    }

    public struct ItemConfig
    {
        public string name;
        public float maxWorth;
        public float mass;
        public float minDamageThreshold;
        public float damageDampener;
        public string rarity;
    }

    string configFile = "Assets/Scripts/Objects/Items/ItemConfig.txt";
    public IDictionary<string, ItemConfig> itemConfigs = new Dictionary<string, ItemConfig>();

    void loadConfig(string filePath)
    {
        // the rest of the lines are formatted like this:
        // itemName,maxWorth,mass,minDamageThreshold,damageDampener,rarity
        string[] lines = System.IO.File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length != 6)
                {
                    Debug.LogError($"Invalid config line on line {i}: {lines[i]}");
                    continue;
                }

                ItemConfig config = new ItemConfig();
                config.name = parts[0];
                config.maxWorth = float.Parse(parts[1]);
                config.mass = float.Parse(parts[2]);
                config.minDamageThreshold = float.Parse(parts[3]);
                config.damageDampener = float.Parse(parts[4]);
                config.rarity = parts[5];
                itemConfigs[config.name] = config;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing config line on line {i}: {lines[i]} - {e.Message}");
            }
        }
    }

}
