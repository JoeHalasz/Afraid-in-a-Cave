using UnityEngine;
using System.Collections.Generic;

public class CreateMap : MonoBehaviour
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

    [SerializeField]
    int firstHallwayPrefabIndex = 0;
    [SerializeField]
    int firstHallwayPrefabConnectionIndex = 0;
    [SerializeField]
    int secondHallwayPrefabIndex = 0;
    [SerializeField]
    int secondHallwayPrefabConnectionIndex = 0;
    

    void alignNewPart(Transform parentB, Transform connectionPointA, Transform connectionPointB)
    {
        Transform parentA = connectionPointA.parent;

        connectionPointA.GetComponent<Renderer>().material.color = Color.magenta;
        connectionPointB.GetComponent<Renderer>().material.color = Color.magenta;

        // calculate the dist between the connections and move parentB so that the connection points are aligned
        Vector3 offset = connectionPointA.position - connectionPointB.position;
        Vector3 newPos = parentB.position + offset;
        parentB.position = newPos;

        // if its off by 90 degrees then rotate it 90 degrees
        if (Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation) == 90)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, -90);
        }
        else if (Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation) == -90)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, 90);
        }
        // make sure the rotations of the connections are always opposites
        if (connectionPointA.rotation == connectionPointB.rotation)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, 180);
        }

    }

    void createMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // Get the map parts from the LoadMapParts script
        LoadMapParts loadMapParts = GetComponent<LoadMapParts>();
        List<LoadMapParts.MapPartData> roomsData = loadMapParts.getRoomsData();
        List<LoadMapParts.MapPartData> hallwaysData = loadMapParts.getHallwaysData();

        List<GameObject> allAvailableConnections = new List<GameObject>();
        int roomCount = 0;
        int hallwayCount = 0;

        // Spawn the first hallway
        GameObject firstHallway = Instantiate(hallwaysData[firstHallwayPrefabIndex].obj, new Vector3(0, 0, 0), Quaternion.identity);
        firstHallway.transform.parent = transform;
        // firstHallway.name = "FirstHallway";

        // Create the second hallway and connect it to the first one
        GameObject secondHallway = Instantiate(hallwaysData[secondHallwayPrefabIndex].obj, Vector3.zero, Quaternion.identity);
        secondHallway.transform.parent = transform;
        secondHallway.name = "SecondHallway";

        // Get the connection points
        Transform firstHallwayConnection = firstHallway.transform.GetChild(firstHallwayPrefabConnectionIndex); // Adjust index if necessary
        Transform secondHallwayConnection = secondHallway.transform.GetChild(secondHallwayPrefabConnectionIndex); // Adjust index if necessary

        // Align the second hallway to the first hallway
        alignNewPart(secondHallway.transform, firstHallwayConnection, secondHallwayConnection);

        return;

        // Add its connections to the list of available connections
        foreach (Transform connection in firstHallway.GetComponentsInChildren<Transform>())
        {
            if (connection.CompareTag("ConnectionPoint"))
            {
                allAvailableConnections.Add(connection.gameObject);
            }
        }

        // Procedurally generate the map
        while (roomCount < 10 && allAvailableConnections.Count > 0 && hallwayCount < 30)
        {
            Debug.Log($"Room count: {roomCount}, Hallway count: {hallwayCount}, Available connections: {allAvailableConnections.Count}");
            // Randomly select an available connection
            GameObject connectionPoint = allAvailableConnections[Random.Range(0, allAvailableConnections.Count)];
            allAvailableConnections.Remove(connectionPoint);

            List<LoadMapParts.MapPartData> roomsToTry = new List<LoadMapParts.MapPartData>();
            List<LoadMapParts.MapPartData> hallwaysToTry = new List<LoadMapParts.MapPartData>();

            if (connectionPoint.transform.parent.name.Contains("Room")) // rooms only connect to hallways
            {
                hallwaysToTry.AddRange(hallwaysData);
            }
            else if (connectionPoint.transform.parent.name.Contains("Hallway")) // hallways connect to anything
            {
                // roomsToTry.AddRange(roomsData);
                hallwaysToTry.AddRange(hallwaysData);
                hallwaysToTry.RemoveAll(x => x.obj.name == connectionPoint.transform.parent.name);
            }

            // try adding rooms until we cant anymore or something was added
            while (roomsToTry.Count != 0 || hallwaysToTry.Count != 0)
            {
                Debug.Log($"Trying connection {connectionPoint.name} on {connectionPoint.transform.parent.name} with {roomsToTry.Count} rooms and {hallwaysToTry.Count} hallways left");
                bool spawnRoom = roomsToTry.Count != 0 && Random.value > .75f;

                GameObject newPart = null;
                if (spawnRoom || hallwaysToTry.Count == 0)
                {
                    spawnRoom = true; // for counting later if there are no hallways
                    int choice = Random.Range(0, roomsToTry.Count);
                    newPart = Instantiate(roomsToTry[choice].obj);
                    roomsToTry.RemoveAt(choice);
                }
                else
                {
                    int choice = Random.Range(0, hallwaysToTry.Count);
                    newPart = Instantiate(hallwaysToTry[choice].obj);
                    hallwaysToTry.RemoveAt(choice);
                }

                List<Transform> newConnectionsToTry = new List<Transform>();
                foreach (Transform connection in newPart.GetComponentsInChildren<Transform>())
                {
                    if (connection.CompareTag("ConnectionPoint"))
                    {
                        newConnectionsToTry.Add(connection);
                    }
                }
                Debug.Log($"New part: {newPart.name} with {newConnectionsToTry.Count} connections");
                bool connectionFound = false;
                Transform newConnection = null;
                while (newConnectionsToTry.Count != 0)
                {
                    // Align the new part using a random connection point on the new part 
                    newConnection = newConnectionsToTry[Random.Range(0, newConnectionsToTry.Count)];
                    newConnectionsToTry.Remove(newConnection);
                    alignNewPart(newPart.transform, connectionPoint.transform, newConnection);

                    // Check for collisions on the new part
                    Collider collider = newPart.GetComponent<Collider>();
                    bool isColliding = false;
                    if (collider != null)
                    {
                        Collider[] colliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, collider.transform.rotation);
                        foreach (Collider c in colliders)
                        {
                            if (c.gameObject != newPart && c.gameObject != connectionPoint)
                            {
                                Debug.Log($"Collision detected with {c.gameObject.name} on {newPart.name}");
                                isColliding = true;
                                break;
                            }
                        }
                    }
                    if (isColliding)
                        continue;
                    connectionFound = true;
                }

                if (!connectionFound)
                {
                    Debug.Log("No connection found for " + newPart.name);
                    Destroy(newPart);
                    continue;
                }

                Debug.Log("No collision detected with " + newPart.name);

                // Add the new part's connections to the list of available connections
                foreach (Transform connection in newPart.GetComponentsInChildren<Transform>())
                {
                    if (connection.CompareTag("ConnectionPoint") && connection.gameObject != newConnection.gameObject)
                    {
                        allAvailableConnections.Add(connection.gameObject);
                    }
                }
                // parent to the connection point
                newPart.transform.parent = connectionPoint.transform;
                // rename it to hallway x or room x
                if (spawnRoom)
                {
                    newPart.name = "Room" + roomCount;
                }
                else
                {
                    newPart.name = "Hallway" + hallwayCount;
                }
                if (newPart.name.Contains("Room"))
                    roomCount++;
                else if (newPart.name.Contains("Hallway"))
                    hallwayCount++;
                break;
            }
        }
    }
}
