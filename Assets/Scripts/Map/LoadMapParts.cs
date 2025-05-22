using UnityEngine;
using System.Collections.Generic;

public class LoadMapParts : MonoBehaviour
{
    public struct MapPartData
    {
        public int number{ get; set; }
        public GameObject obj { get; set; } // prefab
        public List<GameObject> Connections { get; set; } // Connections in the prefab
    }

    List<MapPartData> roomsData = new List<MapPartData>();
    List<MapPartData> hallwaysData = new List<MapPartData>();
    List<MapPartData> extrasData = new List<MapPartData>();

    public List<MapPartData> getRoomsData() { return roomsData; }
    public List<MapPartData> getHallwaysData() { return hallwaysData; }
    public List<MapPartData> getExtrasData() { return extrasData; }

    void Start()
    {
        Debug.Log("Loading map parts...");
        foreach (GameObject obj in Resources.LoadAll<GameObject>("Prefabs/Map/MapParts"))
        {
            if (obj != null)
            {
                MapPartData data = new MapPartData();
                data.obj = obj;
                data.number = roomsData.Count + hallwaysData.Count;
                data.Connections = new List<GameObject>();
                foreach (Transform child in obj.transform)
                    data.Connections.Add(child.gameObject);
                if (obj.name.Contains("Room"))
                    roomsData.Add(data);
                else if (obj.name.Contains("Hallway"))
                    hallwaysData.Add(data);
                else if (obj.name.Contains("Extra"))
                    extrasData.Add(data);
            }
        }
        
    }

}
