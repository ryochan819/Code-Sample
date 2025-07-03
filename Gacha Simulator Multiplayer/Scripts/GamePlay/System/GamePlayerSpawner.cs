using System.Collections.Generic;
using CC;
using Cysharp.Threading.Tasks;
using Gacha.system;
using Gacha.ui;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

namespace Gacha.gameplay
{
    public class GamePlayerSpawner : NetworkBehaviour
    {
        public static GamePlayerSpawner instance;
        [SerializeField] GameObject characterCreator;
        [SerializeField] GameObject mainCam;

        [SerializeField] Transform[] spawnPoints;

        [SerializeField] GameObject femalePlayerPrefab;
        [SerializeField] GameObject malePlayerPrefab;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private UniTask OnGameObjectSpawn()
        {
            if (InstanceHandler.NetworkManager.isServer)
            {
                if (DataPersistenceManager.instance.SaveData.localPlayerData == null)
                {
                    Debug.Log("IsServer No local player data found. Creating new character.");
                    mainCam.SetActive(false);
                    characterCreator.SetActive(true);
                }
                else if (DataPersistenceManager.instance.SaveData.localPlayerData._cc_CharacterData != null)
                {
                    // spawn and setup character
                    // also update localplayer ID
                    Debug.Log("IsServer Found local player data. Spawning character.");
                }
            }
            else
            {
                string playerSteamID = Steamworks.SteamUser.GetSteamID().ToString();
                var multiplayerDatas = GameSceneDataManager.instance.multiplayerDatas.value;

                PlayerDataNetwork? matchingData = multiplayerDatas.Find(data => data.SteamID == playerSteamID);

                if (matchingData.HasValue && !string.IsNullOrEmpty(matchingData.Value.SteamID))
                {
                    // spawn and setup character
                    // also update localplayer ID
                    Debug.Log("Not host Found local player data. Spawning character.");
                }
                else
                {
                    mainCam.SetActive(false);
                    characterCreator.SetActive(true);
                }
            }

            return UniTask.CompletedTask;
        }

        public async UniTask CreateCharacter(CC_CharacterData characterData)
        {
            Debug.Log($"CharacterName: {characterData.CharacterName}");
            Debug.Log($"CharacterPrefab: {characterData.CharacterPrefab}");

            CC_CharacterDataNetwork cc_CharacterDataNetwork = Utility.ConvertToNetworkData(characterData);

            if (InstanceHandler.NetworkManager.isServer)
            {
                characterData.CharacterName = "localPlayer";
                DataPersistenceManager.instance.SaveData.localPlayerData = new PlayerData("", characterData);
                GameSceneDataManager.instance.PlayerDataUpdate("", cc_CharacterDataNetwork, localPlayer);
            }
            else
            {
                string playerSteamID = Steamworks.SteamUser.GetSteamID().ToString();
                characterData.CharacterName = playerSteamID;
                SendCharacterDataToServerRpc(playerSteamID, cc_CharacterDataNetwork, localPlayer);
            }

            var prefabToSpawn = characterData.CharacterPrefab.Contains("Female")? femalePlayerPrefab : malePlayerPrefab;

            Transform spawnPoint = spawnPoints[InstanceHandler.NetworkManager.localPlayer.id];
            Debug.Log($"Game player spawner isSpawned: {isSpawned}");
            var player = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            Debug.Log("Player spawned, setting appearance...");
            await player.GetComponent<PlayerAppearenceController>().SetAppearence(cc_CharacterDataNetwork);
            Debug.Log("Character appearance set complete for " + characterData.CharacterName);

            characterCreator.SetActive(false);
            mainCam.SetActive(true);
            Debug.Log("Character creator deactivated, main camera activated.");
            UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.idle);
        }

        [ServerRpc]
        private void SendCharacterDataToServerRpc(string steamID, CC_CharacterDataNetwork characterDataNetwork, PlayerID? localPlayer)
        {
            // var characterData = Utility.ConvertToLocalData(characterDataNetwork);
            GameSceneDataManager.instance.PlayerDataUpdate(steamID, characterDataNetwork, localPlayer);
        }

        void OnEnable()
        {
            GameEventSystem.OnGameObjectSpawn += OnGameObjectSpawn;
        }

        void OnDisable()
        {
            GameEventSystem.OnGameObjectSpawn -= OnGameObjectSpawn;
        }
    }

    public struct CC_CharacterDataNetwork : IPackedAuto
    {
        public string CharacterName;
        public string CharacterPrefab;

        public List<CC_PropertyNetwork> Blendshapes;
        public List<string> HairNames;
        public List<string> ApparelNames;
        public List<int> ApparelMaterials;

        public List<CC_PropertyNetwork> FloatProperties;
        public List<CC_PropertyNetwork> TextureProperties;
        public List<CC_PropertyNetwork> ColorProperties;
    }

    public struct CC_PropertyNetwork : IPackedAuto
    {
        public string propertyName;
        public string stringValue;
        public float floatValue;
        public Color colorValue;
        public int materialIndex;
        public string meshTag;
    }
}