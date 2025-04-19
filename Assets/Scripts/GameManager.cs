using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    void Awake() { Instance = this; }

    GameObject networkManager;
    GameObject mapManager;

    [SerializeField]
    int sessionNumber = 1;
    [SerializeField] 
    bool connect = false;

    [SerializeField]
    bool createMap = false;

    void Start()
    {
        networkManager = GameObject.Find("NetworkManager");
        mapManager = GameObject.Find("MapManager");
    }

    public int getSeed()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            SyncVars playerSyncVars = player.GetComponent<SyncVars>();
            if (playerSyncVars.isHost.Value)
            {
                Debug.Log("Seed is " + playerSyncVars.seed.Value);
                return playerSyncVars.seed.Value;
            }
        }
        Debug.LogError("No host found, returning -1");
        return -1;
    }

    void FixedUpdate()
    {
        if (connect)
        {
            connect = false;
            onConnectPress();
        }
    }

    async void onConnectPress()
    {
        Debug.Log($"Joining session Session{sessionNumber}");
        await networkManager.GetComponent<SessionManager>().startSession(sessionNumber);
        if (!networkManager.GetComponent<SessionManager>().isConnected)
            Debug.LogError($"Could not connect to {sessionNumber}");
    }

}
