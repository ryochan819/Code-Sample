using System;
using System.Collections;
using Gacha.gameplay;
using Gacha.system;
using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using PurrNet.Transports;
using UnityEngine;

namespace Gacha.Network
{
    public class SteamTransportSetup : MonoBehaviour
    {
        public static SteamTransportSetup instance;

        [SerializeField] SteamTransport steamTransport;
        [SerializeField] UDPTransport udpTransport;

        [SerializeField] private NetworkManager _networkManager;
        private LobbyDataHolder _lobbyDataHolder;
        public LobbyDataHolder LobbyDataHolder => _lobbyDataHolder;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;

            if (!SteamManager.Initialized)
            {
                Debug.Log("SteamManager is not initialized. Please ensure Steam is running and the SteamManager is set up correctly.");
                return;
            }
            

            // if (!TryGetComponent(out _networkManager))
            // {
            //     PurrLogger.Log($"Failed to get {nameof(NetworkManager)} component.", this);
            // }

            _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
            if (!_lobbyDataHolder)
                Debug.Log($"Failed to get {nameof(LobbyDataHolder)} component.", this);
        }

        private void Start()
        {
            if (!_networkManager)
            {
                Debug.LogError($"Failed to start connection. {nameof(NetworkManager)} is null!", this);
                return;
            }
            
            if (!_lobbyDataHolder)
            {
                Debug.LogError($"Failed to start connection. {nameof(LobbyDataHolder)} is null!", this);
                return;
            }

            if (!GameManager.isMultiplayer)
            {
                // Single Player
                _networkManager.transport = udpTransport;
                _networkManager.StartServer();
                StartCoroutine(StartClient());
                return;
            }
            
            if (!_lobbyDataHolder.CurrentLobby.IsValid)
            {
                Debug.Log($"Failed to start connection. Lobby is invalid!", this);
                return;
            }

            if(_lobbyDataHolder.CurrentLobby.IsOwner)
            {
                steamTransport.address = Steamworks.SteamUser.GetSteamID().ToString();
                Debug.Log($"Steam Transport set to Steam ID: {steamTransport.address}");
                _networkManager.StartServer();
                Debug.Log("Is Host start server");
                
                StartCoroutine(StartClient());
                
                Debug.Log("Start client");
            }
            else
            {
                steamTransport.address = _lobbyDataHolder.CurrentLobby.OwnerId;
                Debug.Log($"Steam Transport set to Steam ID: {steamTransport.address}");

                Debug.Log("Start client");
                StartCoroutine(StartClient());
            }
            
        }

        private IEnumerator StartClient()
        {
            yield return new WaitForSeconds(1f);
            _networkManager.StartClient();
        }

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
}
