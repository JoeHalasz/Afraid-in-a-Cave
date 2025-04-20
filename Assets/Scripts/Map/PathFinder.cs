using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PathFinder : NetworkBehaviour
{
    Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();

    CreateMap mapCreator = null;
    GameObject player = null;

    public string target = "Hallway0"; // default to the start

    void Start()
    {
        mapCreator = GameObject.Find("MapManager").GetComponent<CreateMap>();
    }

    public List<string> FindPath(string start, string end)
    {
        graph = mapCreator.getConnections();

        if (!graph.ContainsKey(start) || !graph.ContainsKey(end))
        {
            Debug.LogError("Start or end node not found in the graph.");
            return null;
        }

        // BFS to find the shortest path
        var queue = new Queue<string>();
        var cameFrom = new Dictionary<string, string>();
        var visited = new HashSet<string>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            if (current == end)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        Debug.LogError("No path found.");
        return null;
    }

    List<string> ReconstructPath(Dictionary<string, string> cameFrom, string current)
    {
        var path = new List<string> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    GameObject findConnectionPoint(string first, string second)
    {
        // find the connection point between two parts

        Transform firstPart = mapCreator.transform.Find(first);
        Transform secondPart = mapCreator.transform.Find(second);        

        if (firstPart == null)
            Debug.LogError($"First part {first} not found in the map.");
        if (secondPart == null)
            Debug.LogError($"Second part {second} not found in the map.");

        ConnectConnectionPoints[] firstPartConnections = firstPart.GetComponentsInChildren<ConnectConnectionPoints>();
        // find the one whos Connection is the second part
        foreach (ConnectConnectionPoints connectionPoint in firstPartConnections)
            if (connectionPoint.connection != null && connectionPoint.connection.transform.parent.name == second)
                return connectionPoint.gameObject;
        Debug.LogError("Connection point not found between " + first + " and " + second);
        return null;
    }

    GameObject playerRoomCurrently = null;
    GameObject nextConnectionPoint1 = null;
    GameObject nextConnectionPoint2 = null;

    Vector3 lineOffset = new Vector3(0, -3.25f, 0);

    bool shouldShow = false;

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        if (Input.GetKeyDown(KeyCode.T))
            shouldShow = !shouldShow;
        if (line1 != null && Time.time - lastLine1DrawTime > 0.05f)
        {
            Destroy(line1);
            line1 = null;
        }
        if (line2 != null && Time.time - lastLine2DrawTime > 0.05f)
        {
            Destroy(line2);
            line2 = null;
        }
        if (!shouldShow) return;
        if (player == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject obj in players)
            {
                if (obj.GetComponent<NetworkObject>().IsOwner)
                {
                    player = obj;
                    break;
                }
            }
        }
        if (player == null) return;
        if (target == null) return;
        
        if (playerRoomCurrently != player.GetComponent<TrackRoom>().currentRoom)
        {
            Debug.Log($"Player {player} room changed: {playerRoomCurrently} -> {player.GetComponent<TrackRoom>().currentRoom}");
            playerRoomCurrently = player.GetComponent<TrackRoom>().currentRoom;
            if (playerRoomCurrently == null) return;
            // update the path to the target
            List<string> path = FindPath(playerRoomCurrently.name, target);
            // find the connection points between the rooms
            if (path != null && path.Count > 1)
                nextConnectionPoint1 = findConnectionPoint(path[0], path[1]);
            else
                nextConnectionPoint1 = null;
            // find the connection point between the next room and the room after that if it exists
            if (path != null && path.Count > 2)
                nextConnectionPoint2 = findConnectionPoint(path[1], path[2]);
            else
                nextConnectionPoint2 = null;
        }
        if (nextConnectionPoint1 != null)
        {
            Vector3 start = player.transform.position;
            Vector3 end = nextConnectionPoint1.transform.position;
            showLine(start, end + lineOffset, ref line1);
            lastLine1DrawTime = Time.time;
            if (nextConnectionPoint2 != null)
            {
                Vector3 start2 = nextConnectionPoint1.transform.position;
                Vector3 end2 = nextConnectionPoint2.transform.position;
                showLine(start2 + lineOffset, end2 + lineOffset, ref line2);
                lastLine2DrawTime = Time.time;
            }
        }
    }

    GameObject line1 = null;
    GameObject line2 = null;
    float lastLine1DrawTime = 0f;
    float lastLine2DrawTime = 0f;

    void showLine(Vector3 start, Vector3 end, ref GameObject line)
    {
        LineRenderer lineRenderer;
        if (line == null)
        {
            line = new GameObject("Line");
            lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.startColor = new Color(0, .3f, .3f, 0.15f);
            lineRenderer.endColor = new Color(0, .3f, .3f, 0.15f);
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            line.transform.parent = mapCreator.transform;
        }
        lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

}
