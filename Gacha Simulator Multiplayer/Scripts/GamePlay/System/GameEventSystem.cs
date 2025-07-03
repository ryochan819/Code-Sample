using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gacha.gameplay
{
    public static class GameEventSystem
    {
        public static event Func<UniTask> OnServerDataSetup;
        public static async UniTask ServerDataSetup()
        {
            if (OnServerDataSetup == null)
            {
                Debug.Log("[GameStartSetup] No handlers registered.");
                return;
            }

            var invocationList = OnServerDataSetup.GetInvocationList();
            List<UniTask> tasks = new List<UniTask>(invocationList.Length);

            foreach (var handler in invocationList)
            {
                if (handler is Func<UniTask> asyncHandler)
                {
                    tasks.Add(asyncHandler());
                }
            }

            Debug.Log($"[GameStartSetup] Invoking {invocationList.Length} handler(s).");

            await UniTask.WhenAll(tasks);
        }

        public static event Func<UniTask> OnGameStartSetup;
        public static async UniTask GameStartSetup()
        {
            if (OnGameStartSetup == null)
            {
                Debug.Log("[GameStartSetup] No handlers registered.");
                return;
            }

            var invocationList = OnGameStartSetup.GetInvocationList();
            List<UniTask> tasks = new List<UniTask>(invocationList.Length);

            foreach (var handler in invocationList)
            {
                if (handler is Func<UniTask> asyncHandler)
                {
                    tasks.Add(asyncHandler());
                }
            }

            Debug.Log($"[GameStartSetup] Invoking {invocationList.Length} handler(s).");

            await UniTask.WhenAll(tasks);
        }

        public static event Func<UniTask> OnGameObjectSpawn;

        public static async UniTask GameObjectSpawn()
        {
            if (OnGameObjectSpawn == null) return;

            var invocationList = OnGameObjectSpawn.GetInvocationList();
            List<UniTask> tasks = new List<UniTask>(invocationList.Length);

            foreach (var handler in invocationList)
            {
                if (handler is Func<UniTask> asyncHandler)
                {
                    tasks.Add(asyncHandler());
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public static event Func<UniTask> OnSpawnedObjectSetup;

        public static async UniTask SpawnedObjectSetup()
        {
            if (OnSpawnedObjectSetup == null) return;

            var invocationList = OnSpawnedObjectSetup.GetInvocationList();
            List<UniTask> tasks = new List<UniTask>(invocationList.Length);

            foreach (var handler in invocationList)
            {
                if (handler is Func<UniTask> asyncHandler)
                {
                    tasks.Add(asyncHandler());
                }
            }

            await UniTask.WhenAll(tasks);
        }

        public static event Action OnGameSetupComplete;
        public static void GameSetupComplete()
        {
            OnGameSetupComplete?.Invoke();
        }

        public static event Action<bool> onEnteredBuildMode;
        public static void EnteredBuildModeEvent(bool enter)
        {
            onEnteredBuildMode?.Invoke(enter);
        }

        public static event Action<PlayerState> onSwitchPlayerState;
        public static void SwitchPlayerState(PlayerState state)
        {
            onSwitchPlayerState?.Invoke(state);
        }
    }
}