using System;
using Cysharp.Threading.Tasks;
using Gacha.system;
using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public class GameSceneController : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("GameEventSystem.OnGameStartSetup subscribed.");
            GameEventSystem.OnGameStartSetup += OnGameStartSetup;
        }

        private async UniTask OnGameStartSetup()
        {
            Debug.Log("Player ID: "+ InstanceHandler.NetworkManager.localPlayer.id + " is server: " + InstanceHandler.NetworkManager.isServer);
            // Step 1: Load GameData
            Debug.Log("Loading Game Data");
            await GameDataLoaded();

            // Step 2: Start loading character and spawning GameObject in parallel
            Debug.Log("Loading character and spawning GameObject...");
            await GameEventSystem.GameObjectSpawn();

            // Step 3: Set data on the spawned GameObject (after both complete)
            Debug.Log("Setting data on spawned GameObject...");
            if (InstanceHandler.NetworkManager.isServer)
            await GameEventSystem.SpawnedObjectSetup();

            // Step 4: Done
            Debug.Log("Game setup complete.");
            GameEventSystem.GameSetupComplete();
        }

        async UniTask GameDataLoaded()
        {
            Debug.Log("Waiting for game data to load...");
            while (!GameSceneDataManager.instance.dataSynced.value)
            {
                await UniTask.Yield();
            }
            Debug.Log("Game data loaded.");
        }

        void OnDestroy()
        {
            GameEventSystem.OnGameStartSetup -= OnGameStartSetup;
        }
    }
}
