using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;


public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    void Awake() { Instance = this; }

    ISession activeSession;

    public ISession ActiveSession
    {
        get => activeSession;
        set
        {
            activeSession = value;
            Debug.Log($"Active session set to {activeSession}");
        }
    }

    public bool isConnected => ActiveSession != null;

    NetworkManager networkManager;

    const string playerNamePropertyKey = "PlayerName";

    int sessionNumber = 0;
    
    void loadSessionFromFile()
    {
        // %appdata%\..\LocalLow\DefaultCompany\Asymmetrical-Horror-Multiplayer-Game
        string sessionFile = $"{Application.persistentDataPath}/session.txt";
        try
        {
            sessionNumber = int.Parse(System.IO.File.ReadAllText(sessionFile));
            Debug.Log($"Loaded session number {sessionNumber} from file {sessionFile}");
        }
        catch (Exception e)
        {
            if (!System.IO.File.Exists(sessionFile))
            {
                System.IO.File.WriteAllText(sessionFile, sessionNumber.ToString());
                Debug.Log($"Created session file {sessionFile} with session number {sessionNumber} at {Application.persistentDataPath}");
            }
            Debug.LogError($"Failed to load session number from file {sessionFile}: {e.Message}");
        }
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
    }

    void OnSessionOwnerPromoted(ulong newOwnerId)
    {
        if (networkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Local client { networkManager.LocalClientId }is now the session owner");
        }
    }

    async void Start()
    {
        loadSessionFromFile();
        // connect to a random session
        await startSession(sessionNumber);
    }

    async public Task startSession(int sessionNumber)
    {
        // Initialize Unity Services if not already initialized
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            Debug.Log("Initializing Unity Services...");
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
                return;
            }
        }

        // If we are connected to a session, leave it
        if (ActiveSession != null)
        {
            await LeaveSession();
        }

        int tries = 0;
        while (tries < 50)
        {
            await Task.Delay(500);
            tries++;
            Debug.Log($"Attempting to create or join session Session{sessionNumber} (try {tries})");
            try
            {
                networkManager = GetComponent<NetworkManager>();
                networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
                networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

                // Check if the player is already signed in
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("Signed in anonymously. PlayerID: " + AuthenticationService.Instance.PlayerId);
                }
                else
                {
                    Debug.Log("Player is already signed in. Skipping sign-in.");
                }

                var options = new SessionOptions()
                {
                    Name = "Session" + sessionNumber,
                    MaxPlayers = 8
                }.WithDistributedAuthorityNetwork();

                ActiveSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync("Session" + sessionNumber, options);
                Debug.Log($"Session {ActiveSession.Id} created with code {ActiveSession.Code}");
                break;
            }
            catch (Exception e)
            {
                // TODO cant do anything about if it says the player is already in a session
                if (e.Message.Contains("has been destroyed but you are still trying to access it."))
                {
                    Debug.LogError("Session has been destroyed. Stopping");
                    break;
                }
                else
                {
                    Debug.LogError($"Failed to create or join session: {e.Message}");
                    sessionNumber++;
                }
            }
        }
    }

    void RegisterSessionEvents()
    {
        // ActiveSession.Changed += OnSessionChanged;
    }
    
    void UnregisterSessionEvents()
    {
        // ActiveSession.Changed -= OnSessionChanged;
    }

    async UniTask<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        try
        {
            var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            var playerNameProperty = new PlayerProperty("PlayerName", VisibilityPropertyOptions.Member);
            var playerIsHostProperty = new PlayerProperty("PlayerIsHost", VisibilityPropertyOptions.Member);
            return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty }, { ActiveSession.IsHost.ToString(), playerIsHostProperty } };
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Failed to get player properties: {e.Message}");
            return null;
        }
    }

    async Task JoinSessionById(string sessionId)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        Debug.Log($"Joined session {ActiveSession.Id} with code {ActiveSession.Code}");
    }

    async Task JoinSessionByCode(string sessionCode)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Joined session {ActiveSession.Id} with code {ActiveSession.Code}");
    }

    async Task KickPlayer(string playerId)
    {
        if (!ActiveSession.IsHost)
        {
            Debug.LogError("Only the host can kick players");
            return;
        }
        await ActiveSession.AsHost().RemovePlayerAsync(playerId);
        Debug.Log($"Kicked player {playerId} from session {ActiveSession.Id}");
    }

    async UniTask LeaveSession()
    {
        if (ActiveSession == null)
        {
            Debug.LogError("No active session to leave");
            return;
        }
        // UnregisterSessionEvents();
        try
        {
            await ActiveSession.LeaveAsync();
            Debug.Log($"Left session {ActiveSession.Id}");
        }
        catch (Exception e) 
        {
            Debug.LogError($"Failed to leave session: {e.Message}");
        }
        finally
        {
            ActiveSession = null;
        }
    }

    // on game object destroy, leave the session
    void OnDestroy()
    {
        if (ActiveSession != null)
        {
            LeaveSession().Forget();
        }
    }

    void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
        if (clientId == networkManager.LocalClientId)
        {
            LeaveSession().Forget();
        }
    }
}
