using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    void Awake() { Instance = this; }

    GameObject networkManager;
    GameObject mapManager;

    [SerializeField]
    string sessionName = "DefaultSession";
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
        return networkManager.GetComponent<SessionManager>().getSeed();
    }

    void FixedUpdate()
    {
        if (connect)
        {
            connect = false;
            onConnectPress();
        }
        if (createMap)
        {
            createMap = false;
            onCreateMapPress();
        }

    }

    async void onConnectPress()
    {
        Debug.Log($"Joining session {sessionName}");
        await networkManager.GetComponent<SessionManager>().startSession(sessionName);
        if (!networkManager.GetComponent<SessionManager>().isConnected)
            Debug.LogError($"Could not connect to {sessionName}");
    }

    void onCreateMapPress()
    {
        // make sure we are connected to a session
        if (!networkManager.GetComponent<SessionManager>().isConnected)
        {
            Debug.LogError("Not connected to a session. Cannot create map.");
            return;
        }
        mapManager.GetComponent<CreateMap>().StartMapCreation = true;
    }

}
