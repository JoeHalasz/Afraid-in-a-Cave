using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

public class SyncVars : NetworkBehaviour
{
    public NetworkVariable<bool> isHost = new NetworkVariable<bool>(false);
    [SerializeField]
    NetworkVariable<int> hostNextStage = new NetworkVariable<int>(-1);
    public NetworkVariable<int> currentStage = new NetworkVariable<int>(0);

    CreateMap createMap;

    public NetworkVariable<int> seed = new NetworkVariable<int>(0);

    [SerializeField]
    bool startMapGen = false;

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        createMap = GameObject.Find("MapManager").GetComponent<CreateMap>();
        if (NetworkManager.LocalClient.IsSessionOwner)
            generateNewSeed();
    }

    public void generateNewSeed()
    {
        seed.Value = (int)System.DateTime.Now.Ticks;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O))
        {
            generateNewSeed();
            startMapGen = true;
        }
    }

    void FixedUpdate()
    {
        if (!HasAuthority || !IsSpawned) return;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (NetworkManager.LocalClient.IsSessionOwner) // host check
        {
            if (startMapGen)
            {
                startMapGen = false;
                hostNextStage.Value = 0;
            }
            isHost.Value = true;
            int hostStage = hostNextStage.Value;
            bool ready = true;
            foreach (GameObject player in players)
                if (hostStage != player.GetComponent<SyncVars>().currentStage.Value)
                    ready = false;
            if (ready)
            {
                hostNextStage.Value++;
                Debug.Log($"Host {NetworkManager.LocalClientId} is starting stage {hostNextStage.Value}");
            }
        }
        int hNextStage = -1;
        foreach (GameObject player in players)
        {
            SyncVars playerSyncVars = player.GetComponent<SyncVars>();
            if (playerSyncVars.isHost.Value)
                hNextStage = playerSyncVars.hostNextStage.Value;
        }
        if (!createMap.startNextStage)
        {
            foreach (GameObject player in players)
            {
                if (hNextStage != -1)
                {
                    Debug.Log($"Client {NetworkManager.LocalClientId} is checking host stage {hNextStage} and current stage {currentStage.Value}");
                    if (hNextStage > currentStage.Value)
                    {
                        createMap.startNextStage = true;
                        Debug.Log($"Client {NetworkManager.LocalClientId} is starting stage {currentStage.Value}");
                        break;
                    }
                }
            }
        }
        if (hNextStage == 0 && currentStage.Value == -1)
        {
            Debug.Log($"Client {NetworkManager.LocalClientId} is restarting map gen");
            createMap.startNextStage = false;
            currentStage.Value = 0;
        }
    }
}