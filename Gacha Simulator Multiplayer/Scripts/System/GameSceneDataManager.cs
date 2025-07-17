using System;
using System.Collections.Generic;
using System.Linq;
using CC;
using Cysharp.Threading.Tasks;
using Gacha.gameplay;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

namespace Gacha.system
{
    public class GameSceneDataManager : NetworkBehaviour
    {
        [Header("LocalPlayerData")]
        PlayerController gameLocalPlayer;
        public PlayerController LocalPlayer => gameLocalPlayer;

        [Header("MultiplayerData")]
        public SyncVar<bool> dataSynced = new(false);
        public SyncVar<List<PlayerDataNetwork>> multiplayerDatas;

        [Header("Shop Data")]
        public SyncVar<int> money = new(30000); // Default starting value
        public SyncVar<int> shopLevel = new(0);

        public static GameSceneDataManager instance;
        private Queue<PlayerDataUpdateRequest> _pendingUpdates = new();
        private bool _isProcessingScheduled = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void OnMoneyChanged(int newValue)
        {
            Debug.Log($"Money updated to {newValue}");
            // UI updates or other responses to money change go here
        }

        public UniTask Load_GameData()
        {
            if (DataPersistenceManager.instance.SaveData.IsNewSave)
            {
                dataSynced.value = true;
                money.value = 30000;
                return UniTask.CompletedTask;
            }

            money.value = DataPersistenceManager.instance.SaveData.money;
            Debug.Log("Set money:" + DataPersistenceManager.instance.SaveData.money);

            List<PlayerDataNetwork> playerDataNetworkList = new List<PlayerDataNetwork>();
            foreach (var data in DataPersistenceManager.instance.SaveData.multiplayerDatas)
            {
                PlayerDataNetwork playerDataNetwork = new PlayerDataNetwork
                {
                    SteamID = data._steamID,
                    characterData = Utility.ConvertToNetworkData(data._cc_CharacterData)
                };
                playerDataNetworkList.Add(playerDataNetwork);
            }
            multiplayerDatas.value = playerDataNetworkList;
            dataSynced.value = true;
            return UniTask.CompletedTask;
        }

        public void SetLocalPlayer(PlayerController player)
        {
            gameLocalPlayer = player;
        }


        public void PlayerDataUpdate(string steamID, CC_CharacterDataNetwork characterData, PlayerID? playerID)
        {
            var updatedList = new List<PlayerDataNetwork>(multiplayerDatas.value);
            bool found = false;

            for (int i = 0; i < updatedList.Count; i++)
            {
                if (updatedList[i].SteamID == steamID)
                {
                    updatedList[i] = new PlayerDataNetwork
                    {
                        SteamID = steamID,
                        characterData = characterData,
                        PlayerID = playerID.HasValue ? playerID.Value.id : (ushort)0
                    };
                    found = true;
                    Debug.Log("PlayerDataUpdated - existing entry updated");
                    break;
                }
            }

            if (!found)
            {
                updatedList.Add(new PlayerDataNetwork
                {
                    SteamID = steamID,
                    characterData = characterData,
                    PlayerID = playerID.HasValue ? playerID.Value.id : (ushort)0
                });

                Debug.Log("PlayerDataUpdated - new entry added");
            }

            // ✅ Re-assign the whole list to trigger dirty flag
            multiplayerDatas.value = updatedList;
        }

        public CC_CharacterDataNetwork GetPlayerCharacterData(ushort playerID)
        {
            var playerData = multiplayerDatas.value.FirstOrDefault(data => data.PlayerID == playerID);
            if (playerData.SteamID != null)
            {
                return playerData.characterData;
            }
            else
            {
                Debug.LogWarning($"No character data found for PlayerID: {playerID}");
                return default;
            }
        }

        public UniTask Save_GameData()
        {
            DataPersistenceManager.instance.SaveData.money = money.value;
            return UniTask.CompletedTask;
        }

        void SendBoardCastTest()
        {
            if (InstanceHandler.NetworkManager.isServer)
            {
                InstanceHandler.NetworkManager.SendToAll(new BoardCastCharacterData());
            }
            else
            {
                InstanceHandler.NetworkManager.SendToServer(new BoardCastCharacterData());
            }
        }

        private void OnBoardCastTestReceived(PlayerID player, BoardCastCharacterData data, bool asServer)
        {

        }

        void OnEnable()
        {
            money.onChanged += OnMoneyChanged;

            InstanceHandler.NetworkManager.Subscribe<BoardCastCharacterData>(OnBoardCastTestReceived);
        }

        void OnDisable()
        {
            money.onChanged -= OnMoneyChanged;

            if (InstanceHandler.TryGetInstance<NetworkManager>(out var networkManager))
            {
                networkManager.Unsubscribe<BoardCastCharacterData>(OnBoardCastTestReceived);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (instance == this)
                instance = null;
        }
    }

    [System.Serializable]
    public struct BoardCastCharacterData : IPackedAuto
    {
        public string SteamID;
        public CC_CharacterData characterData;
    }

    [System.Serializable]
    public struct PlayerDataNetwork : IPackedAuto
    {
        public string SteamID;
        public CC_CharacterDataNetwork characterData;
        public ushort PlayerID;
    }

    public struct PlayerDataUpdateRequest
    {
        public string steamID;
        public CC_CharacterDataNetwork characterData;

        public PlayerDataUpdateRequest(string id, CC_CharacterDataNetwork data)
        {
            steamID = id;
            characterData = data;
        }
    }
}
