using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CreateMap : MonoBehaviour
{
    public static CreateMap Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    bool StartMapCreation = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (StartMapCreation)
        {
            StartMapCreation = false;
            // spawn a thread to create the map
            Debug.Log("Creating map...");
            StartCoroutine(createMap());
        }
    }

    [SerializeField]
    int firstHallwayPrefabIndex = 0;    
    
    public void alignNewPart(Transform parentB, Transform connectionPointA, Transform connectionPointB)
    {
        Transform parentA = connectionPointA.parent;

        connectionPointA.GetComponent<Renderer>().material.color = Color.magenta;
        connectionPointB.GetComponent<Renderer>().material.color = Color.magenta;

        // calculate the dist between the connections and move parentB so that the connection points are aligned
        Vector3 offset = connectionPointA.position - connectionPointB.position;
        Vector3 newPos = parentB.position + offset;
        parentB.position = newPos;
        // if its off by 90 degrees then rotate it 90 degrees
        if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == 90)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, -90);
        }
        else if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == -90)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, 90);
        }
        // if its off by 270 degrees then rotate it 90 degrees
        else if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == 270)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, -270);
        }
        else if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == -270)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, 270);
        }
        // if the rotations are really close, then rotate it 180 degrees
        if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == 0)
        {
            parentB.RotateAround(connectionPointB.position, Vector3.up, 180);
        }
        // move the parents slightly away from each other to avoid collision
        Vector3 direction = (parentB.position - parentA.position).normalized;
        float distance = Vector3.Distance(parentA.position, parentB.position);
        float offsetDistance = 0.1f; // adjust this value to control the distance between the two parents
        parentB.position += direction * offsetDistance;
    }

    List<LoadMapParts.MapPartData> roomsData;
    List<LoadMapParts.MapPartData> hallwaysData;
    List<GameObject> allAvailableConnections = new List<GameObject>();
    private List<Bounds> collisionBounds = new List<Bounds>();

    public void addPartToConnection(GameObject connectionPoint, int newPartIndex, int newPartConnectionPointIndex)
    {
        if (roomsData == null || hallwaysData == null)
        {
            Debug.Log("Map has not been created yet");
            return;
        }
        // make sure the indicies are good 
        if (newPartIndex < 0 || newPartIndex >= hallwaysData.Count)
        {
            Debug.Log("Invalid part index: " + newPartIndex);
            return;
        }
        if (newPartConnectionPointIndex < 0 || newPartConnectionPointIndex >= hallwaysData[newPartIndex].Connections.Count)
        {
            Debug.Log("Invalid connection point index: " + newPartConnectionPointIndex);
            return;
        }
        // spawn it in 
        GameObject newPart = Instantiate(hallwaysData[newPartIndex].obj, connectionPoint.transform.position, connectionPoint.transform.rotation);
        newPart.transform.parent = connectionPoint.transform;
        newPart.name = "Hallway" + totalParts;
        // get the connection point on the new part
        Transform newPartConnectionPoint = newPart.transform.GetChild(newPartConnectionPointIndex);
        // align the new part to the old part
        alignNewPart(newPart.transform, connectionPoint.transform, newPartConnectionPoint);
        addCollider(newPart);
        totalParts++;
    }

    void addCollider(GameObject newPart)
    {
        if (newPart.GetComponent<Rigidbody>() == null)
        {
            newPart.AddComponent<Rigidbody>();
            newPart.GetComponent<Rigidbody>().isKinematic = true;
        }
        if (newPart.GetComponent<BoxCollider>() == null)
        {
            BoxCollider boxCollider = newPart.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
    }

    int totalParts = 1;
    IEnumerator createMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        // Get the map parts from the LoadMapParts script
        LoadMapParts loadMapParts = GetComponent<LoadMapParts>();
        roomsData = loadMapParts.getRoomsData();
        hallwaysData = loadMapParts.getHallwaysData();

        allAvailableConnections.Clear();

        int roomCount = 0;
        int hallwayCount = 0;
        totalParts = 1;

        // Spawn the first hallway
        GameObject firstHallway = Instantiate(hallwaysData[firstHallwayPrefabIndex].obj, new Vector3(0, 0, 0), Quaternion.identity);
        firstHallway.transform.parent = transform;
        firstHallway.name = "Hallway0";
        addCollider(firstHallway);

        // Add its connections to the list of available connections
        foreach (Transform connection in firstHallway.GetComponentsInChildren<Transform>())
        {
            if (connection.CompareTag("ConnectionPoint"))
            {
                allAvailableConnections.Add(connection.gameObject);
            }
        }

        // Procedurally generate the map
        while (roomCount < 10 && allAvailableConnections.Count > 0 && hallwayCount < 300)
        {
            // Debug.Log($"Room count: {roomCount}, Hallway count: {hallwayCount}, Available connections: {allAvailableConnections.Count}");
            // Randomly select an available connection
            GameObject connectionPoint = allAvailableConnections[Random.Range(0, allAvailableConnections.Count)];
            allAvailableConnections.Remove(connectionPoint);
            // make sure it still exists
            if (connectionPoint == null)
                continue;

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
                // make sure this connection point stil exists
                if (connectionPoint == null)
                    break;
                // Debug.Log($"Trying connection {connectionPoint.name} on {connectionPoint.transform.parent.name} with {roomsToTry.Count} rooms and {hallwaysToTry.Count} hallways left");
                bool spawnRoom = roomsToTry.Count != 0 && Random.value > .75f;

                GameObject newPart = null;
                if (spawnRoom || hallwaysToTry.Count == 0)
                {
                    spawnRoom = true; // for counting later if there are no hallways
                    int choice = Random.Range(0, roomsToTry.Count);
                    newPart = Instantiate(roomsToTry[choice].obj);
                    roomsToTry.RemoveAt(choice);
                    newPart.name = "Room" + totalParts;
                }
                else
                {
                    int choice = Random.Range(0, hallwaysToTry.Count);
                    newPart = Instantiate(hallwaysToTry[choice].obj);
                    hallwaysToTry.RemoveAt(choice);
                    newPart.name = "Hallway" + totalParts;
                }
                totalParts++;
                newPart.transform.parent = connectionPoint.transform;

                List<Transform> newConnectionsToTry = new List<Transform>();
                foreach (Transform connection in newPart.GetComponentsInChildren<Transform>())
                {
                    if (connection.CompareTag("ConnectionPoint"))
                    {
                        newConnectionsToTry.Add(connection);
                    }
                }
                // Debug.Log($"New part: {newPart.name} with {newConnectionsToTry.Count} connections");
                bool connectionFound = false;
                Transform newConnection = null;
                while (newConnectionsToTry.Count != 0)
                {
                    if (connectionPoint == null || newPart == null)
                        break;
                    // Align the new part using a random connection point on the new part 
                    newConnection = newConnectionsToTry[Random.Range(0, newConnectionsToTry.Count)];
                    newConnectionsToTry.Remove(newConnection);
                    if (newConnection == null)
                        continue;
                    alignNewPart(newPart.transform, connectionPoint.transform, newConnection);
                    addCollider(newPart);
                    yield return new WaitForSeconds(1f/30f); // TODO see how fast this can go
                    if (newPart == null)
                    {
                        continue;
                    }
                    else
                    {
                        connectionFound = true;
                    }
                }

                if (!connectionFound || newPart == null)
                {
                    // Debug.Log("No connection found for " + newPart.name);
                    // newPart.GetComponent<Renderer>().material.color = Color.red;
                    // Destroy(newPart);
                    continue;
                }
                else
                {
                    // Debug.Log("No collision detected with " + newPart.name);
                    // Add the new part's connections to the list of available connections
                    foreach (Transform connection in newPart.GetComponentsInChildren<Transform>())
                    {
                        if (connection.CompareTag("ConnectionPoint") && connection.gameObject != newConnection.gameObject)
                        {
                            allAvailableConnections.Add(connection.gameObject);
                        }
                    }
                    if (newPart.name.Contains("Room"))
                        roomCount++;
                    else if (newPart.name.Contains("Hallway"))
                        hallwayCount++;
                    break;
                }
            }
        }
        Debug.Log($"Stopped because room count: {roomCount}, hallway count: {hallwayCount}, available connections: {allAvailableConnections.Count}");
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Debug.Log("Total collision bounds: " + collisionBounds.Count);
    //     foreach (Bounds bounds in collisionBounds)
    //     {
    //         Debug.Log("Drawing bounds: " + bounds);
    //         Gizmos.DrawWireCube(bounds.center, bounds.size);
    //     }
    // }
}
