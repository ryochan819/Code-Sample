using Gacha.gameplay;
using Gacha.system;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;

public class NetworkLocalPlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;

    private void OnServerConnectionState(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            Debug.Log("Server Connected to client.");
            GameSceneDataManager.instance.Load_GameData();
        }
        else if (state == ConnectionState.Disconnected)
        {
            Debug.Log("Server Disconnected from client.");
        }
    }

    private void OnClientConnectionState(ConnectionState state)
    {
        Debug.Log($"Connection state changed: {state}");
        if (state == ConnectionState.Connected)
        {
            Debug.Log("Client Connected to server.");
            Debug.Log("Game Start Setup");
            _ = GameEventSystem.GameStartSetup();
        }
    }

    void OnEnable()
    {
        _networkManager.onServerConnectionState += OnServerConnectionState;
        _networkManager.onClientConnectionState += OnClientConnectionState;
    }

    void OnDisable()
    {
        _networkManager.onServerConnectionState -= OnServerConnectionState;
        _networkManager.onClientConnectionState -= OnClientConnectionState;
    }
}
