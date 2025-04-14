using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

public class CreateMap : MonoBehaviour
{

    [SerializeField]
    int maxRooms = 30;
    [SerializeField]
    float moneyNeeded = 5000f;
    [SerializeField]
    public bool StartMapCreation = false;
    [SerializeField]
    bool stopGeneration = false;
    [SerializeField]
    bool redoReplace = false;
    List<LoadMapParts.MapPartData> roomsData;
    List<LoadMapParts.MapPartData> hallwaysData;
    List<GameObject> allAvailableConnections = new List<GameObject>();
    private List<Bounds> collisionBounds = new List<Bounds>();
    int totalParts = 1;
    bool mapPlanComplete = false;
    bool replacementComplete = false;
    bool spawnItemsComplete = false;
    ReplaceMapParts replaceMapParts = null;
    ItemManager itemManager = null;

    void Start()
    {
        replaceMapParts = GetComponent<ReplaceMapParts>();
        itemManager = GetComponent<ItemManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (StartMapCreation)
        {
            StartMapCreation = false;
            int seed = GameManager.Instance.getSeed();
            if (seed == 0)
            {
                Debug.LogError("Seed is not set. Please set the seed");
                return;
            }
            Debug.Log($"Seed is {(int)seed}");
            Random.InitState((int)seed);
            // spawn a thread to create the map
            Debug.Log("Creating map...");
            StartCoroutine(createMapPlan());
        }
        if (mapPlanComplete || redoReplace)
        {
            redoReplace = false;
            mapPlanComplete = false;
            // start the next step
            StartCoroutine(createMapPlanThread());
        }
        if (replacementComplete)
        {
            replacementComplete = false;
            spawnItems();
        }
        if (spawnItemsComplete)
        {
            spawnItemsComplete = false;
            unloadMap();
        }
    }

    IEnumerator createMapPlanThread()
    {
        yield return 20;
        replaceMapParts.replaceParts();
        replacementComplete = true;
    }

    void spawnItems()
    {
        itemManager.spawnItems(moneyNeeded);
        spawnItemsComplete = true;
    }

    void unloadMap()
    {
        foreach (Transform child in transform)
        {
            Debug.Log("Unloading: " + child.name);
            LoadNearMapParts loadNearMapParts = child.GetComponentInChildren<LoadNearMapParts>();
            if (loadNearMapParts != null)
                loadNearMapParts.checkShouldBeLoaded();
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
            player.transform.position = new Vector3(0, 0, 0);
    }

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
            parentB.RotateAround(connectionPointB.position, Vector3.up, -90);
        else if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == -90)
            parentB.RotateAround(connectionPointB.position, Vector3.up, 90);
        if (Mathf.Round(Quaternion.Angle(connectionPointA.rotation, connectionPointB.rotation)) == 0)
            parentB.RotateAround(connectionPointB.position, Vector3.up, 180);
    }

    public GameObject addPartToConnection(GameObject connectionPoint, int newPartIndex, int newPartConnectionPointIndex)
    {
        if (roomsData == null || hallwaysData == null)
        {
            Debug.Log("Map has not been created yet");
            return null;
        }
        // make sure the indicies are good 
        if (newPartIndex < 0 || newPartIndex >= hallwaysData.Count)
        {
            Debug.Log("Invalid part index: " + newPartIndex);
            return null;
        }
        if (newPartConnectionPointIndex < 0 || newPartConnectionPointIndex >= hallwaysData[newPartIndex].Connections.Count)
        {
            Debug.Log("Invalid connection point index: " + newPartConnectionPointIndex);
            return null;
        }
        // spawn it in 
        GameObject newPart = Instantiate(hallwaysData[newPartIndex].obj, connectionPoint.transform.position, connectionPoint.transform.rotation);
        newPart.transform.parent = transform;
        newPart.GetComponent<SavedPartType>().partTypeName = newPart.name.Replace("(Clone)", "");
        newPart.name = "Hallway" + totalParts;
        // get the connection point on the new part
        Transform newPartConnectionPoint = newPart.transform.GetChild(newPartConnectionPointIndex);
        // align the new part to the old part
        alignNewPart(newPart.transform, connectionPoint.transform, newPartConnectionPoint);
        addCollider(newPart);
        totalParts++;
        return newPart;
    }

    void addCollider(GameObject newPart)
    {
        if (newPart.GetComponent<Rigidbody>() == null)
        {
            newPart.AddComponent<Rigidbody>();
            newPart.GetComponent<Rigidbody>().isKinematic = true;
            newPart.GetComponent<Rigidbody>().useGravity = false;
            newPart.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        if (newPart.GetComponent<BoxCollider>() == null)
        {
            BoxCollider boxCollider = newPart.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(.99f,.99f,.99f);
            boxCollider.center = new Vector3(0, 0, 0);
        }
        foreach (Transform child in newPart.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("ConnectionPoint"))
            {
                if (child.GetComponent<BoxCollider>() == null)
                {
                    BoxCollider boxCollider = child.gameObject.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true;
                    boxCollider.size = new Vector3(1,1,1);
                    boxCollider.center = new Vector3(0, 0, 0);
                }
                if (child.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
            }
        }
    }

    IEnumerator createMapPlan()
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
        totalParts = 0;

        GameObject enterance = Instantiate(hallwaysData[3].obj, new Vector3(0, 0, 0), Quaternion.identity);
        enterance.transform.parent = transform;
        enterance.GetComponent<SavedPartType>().partTypeName = enterance.name.Replace("(Clone)", "");
        enterance.name = "Hallway" + totalParts++;
        addCollider(enterance);
        enterance = addPartToConnection(enterance.transform.GetChild(0).gameObject, 3, 1);
        addCollider(enterance);
        GameObject firstHallway = addPartToConnection(enterance.transform.GetChild(0).gameObject, 1, 2);
        // add the second connection point of that hallway to the list of available connections
        allAvailableConnections.Add(firstHallway.transform.GetChild(0).gameObject);
        allAvailableConnections.Add(firstHallway.transform.GetChild(1).gameObject);

        // Procedurally generate the map
        while (roomCount < maxRooms && allAvailableConnections.Count > 0)
        {
            if (stopGeneration)
            {
                Debug.Log("Stopping generation");
                stopGeneration = false;
                break;
            }
            // Debug.Log($"Room count: {roomCount}, Hallway count: {hallwayCount}, Available connections: {allAvailableConnections.Count}");
            GameObject connectionPoint = allAvailableConnections[0];
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
                roomsToTry.AddRange(roomsData);
                foreach (LoadMapParts.MapPartData hallway in hallwaysData)
                {
                    // if there materials colors are not the same then add it to the list 
                    if (!hallway.obj.transform.GetComponent<Renderer>().sharedMaterial.color.Equals(connectionPoint.transform.parent.GetComponent<Renderer>().material.color))
                    {
                        hallwaysToTry.Add(hallway);
                    }
                }
            }

            // try adding rooms until we cant anymore or something was added
            while (roomsToTry.Count != 0 || hallwaysToTry.Count != 0)
            {
                // make sure this connection point stil exists
                if (connectionPoint == null)
                    break;
                bool spawnRoom = roomsToTry.Count != 0 && Random.value > .25f;

                GameObject newPart = null;
                if (spawnRoom || hallwaysToTry.Count == 0)
                {
                    spawnRoom = true; // for counting later if there are no hallways
                    int choice = Random.Range(0, roomsToTry.Count);
                    newPart = Instantiate(roomsToTry[choice].obj);
                    roomsToTry.RemoveAt(choice);
                    newPart.GetComponent<SavedPartType>().partTypeName = newPart.name.Replace("(Clone)", "");
                    newPart.name = "Room" + totalParts;
                }
                else
                {
                    int choice = Random.Range(0, hallwaysToTry.Count);
                    newPart = Instantiate(hallwaysToTry[choice].obj);
                    hallwaysToTry.RemoveAt(choice);
                    newPart.GetComponent<SavedPartType>().partTypeName = newPart.name.Replace("(Clone)", "");
                    newPart.name = "Hallway" + totalParts;
                }
                totalParts++;
                newPart.transform.parent = transform;

                List<Transform> newConnectionsToTry = new List<Transform>();
                foreach (Transform connection in newPart.GetComponentsInChildren<Transform>())
                {
                    if (connection.CompareTag("ConnectionPoint"))
                    {
                        newConnectionsToTry.Add(connection);
                    }
                }
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
                    // if the new part is higher than the first part then delete it and move on
                    if (newPart.transform.position.y > firstHallway.transform.position.y)
                    {
                        connectionFound = false;
                        continue;
                    }
                    addCollider(newPart);
                    // wait 1 frame
                    yield return 0;
                    if (newPart == null)
                        continue;
                    else
                        connectionFound = true;
                }

                if (!connectionFound || newPart == null)
                {
                    if (newPart != null)
                        Destroy(newPart);
                    continue;
                }
                else
                {
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
        mapPlanComplete = true;
    }

}
