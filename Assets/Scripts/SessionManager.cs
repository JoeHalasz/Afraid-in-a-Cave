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

    string sessionName = "MySession";

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
        // connect to a random session
        sessionName = "Session2";// + UnityEngine.Random.Range(0, 10000).ToString();
        await startSession(sessionName);
    }

    async public Task startSession(string sessionName)
    {
        // if we are connected to a session then leave it
        if (ActiveSession != null)
        {
            await LeaveSession();
        }
        try
        {
            networkManager = GetComponent<NetworkManager>();
            networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously. PlayerID: " + AuthenticationService.Instance.PlayerId);

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = 8
            }.WithDistributedAuthorityNetwork();

            ActiveSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

            Debug.Log($"Session {ActiveSession.Id} created with code {ActiveSession.Code}");
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Failed to sign in anonymously: {e.Message}");
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

    async UniTaskVoid JoinSessionById(string sessionId)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        Debug.Log($"Joined session {ActiveSession.Id} with code {ActiveSession.Code}");
    }

    async UniTaskVoid JoinSessionByCode(string sessionCode)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Joined session {ActiveSession.Id} with code {ActiveSession.Code}");
    }

    async UniTaskVoid KickPlayer(string playerId)
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
        catch { }
        finally
        {
            ActiveSession = null;
        }
    }

    // on game quit or application quit, leave the session
    void OnApplicationQuit()
    {
        if (ActiveSession != null)
        {
            LeaveSession().Forget();
        }
    }

}
