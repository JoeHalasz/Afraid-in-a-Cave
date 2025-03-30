using UnityEngine;
using System.Collections.Generic;

public class CreateMap2 : MonoBehaviour
{
    [SerializeField]
    bool StartMapCreation = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (StartMapCreation)
        {
            StartMapCreation = false;
            createMap();
        }
    }
    
    // enum for north south east west
    public enum Direction
    {
        North,
        South,
        East,
        West,
        None
    }

    private struct ConnectionPossibility
    {
        public GameObject obj;
        public Direction direction;
    }

    // function to add all directions to a list of connection possibilities( pass by reference )
    private void addDirectionsToList(ref List<ConnectionPossibility> list, GameObject obj, Direction exclude=Direction.None)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == (int)exclude) continue; // skip the excluded direction
            ConnectionPossibility connection = new ConnectionPossibility();
            connection.obj = obj;
            connection.direction = (Direction)i;
            list.Add(connection);
        }
    }

    void createMap()
    {
        // Get the map parts from the LoadMapParts script
        LoadMapParts loadMapParts = GetComponent<LoadMapParts>();
        List<LoadMapParts.MapPartData> roomsData = loadMapParts.getRoomsData();
        List<LoadMapParts.MapPartData> hallwaysData = loadMapParts.getHallwaysData();

        List<GameObject> allAvailableConnections = new List<GameObject>();
        int roomCount = 0;
        int hallwayCount = 0;

        // Spawn the first hallway
        GameObject firstHallway = Instantiate(hallwaysData[0].obj, new Vector3(0, 0, 0), Quaternion.identity);
        firstHallway.transform.parent = transform;

        // assume that every other map part can be connected on its north, south, east and west sides. 
        // rooms can only connect to hallways, and hallways can connect to rooms or other hallways.
        
        int numRooms = 0;
        int numHallways = 0;
        
        List<ConnectionPossibility> possibleConnections = new List<ConnectionPossibility>();
        addDirectionsToList(ref possibleConnections, firstHallway);

        while (numRooms < 10 && numHallways < 10 && possibleConnections.Count != 0)
        {
            // pick a random connection possibility from the list
            int randIndex = Random.Range(0, possibleConnections.Count);
            ConnectionPossibility connection = possibleConnections[randIndex];
            possibleConnections.RemoveAt(randIndex); // remove the connection from the list

            // try every avai

        }
        
    }
}
