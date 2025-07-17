using System;
using System.Collections;
using CC;
using Cysharp.Threading.Tasks;
using Gacha.system;
using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public class PlayerAppearenceController : NetworkBehaviour
    {
        [SerializeField] CharacterCustomization characterCustomization;

        bool isOnSpawned = false;
        bool appearenceSetup = false;

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned();

            if (asServer) return;

            isOnSpawned = true;
        }

        public async UniTask SetAppearence(CC_CharacterDataNetwork cc_CharacterNetworkData)
        {
            appearenceSetup = true;

            while (!isOnSpawned)
            {
                await UniTask.WaitForEndOfFrame();
                Debug.Log("Waiting for network identity..." + isOnSpawned);
            }

            var tcs = new UniTaskCompletionSource();

            void OnLoaded(CharacterCustomization _)
            {
                tcs.TrySetResult();
            }

            characterCustomization.onCharacterLoaded += OnLoaded;

            // Fire RPC (does not await)
            SetAppearenceObserversRpc(cc_CharacterNetworkData);

            // Wait for onCharacterLoaded to fire
            await tcs.Task;

            characterCustomization.onCharacterLoaded -= OnLoaded;

            Debug.Log("Character appearance set complete!");
        }

        [ObserversRpc(bufferLast: true)]
        public void SetAppearenceObserversRpc(CC_CharacterDataNetwork cc_CharacterNetworkData)
        {
            var characterDataLocal = Utility.ConvertToLocalData(cc_CharacterNetworkData);
            Debug.Log($"RPC [PlayerAppearenceController] Setting appearance for {characterDataLocal.CharacterName}");
            characterCustomization.ApplyCharacterVars(characterDataLocal);
            Debug.Log("RPC Character appearance set complete");
        }

        public void SetAppearenceObservers(CC_CharacterDataNetwork cc_CharacterNetworkData)
        {
            var characterData = Utility.ConvertToLocalData(cc_CharacterNetworkData);
            Debug.Log($"[PlayerAppearenceController] Setting appearance for {characterData.CharacterName}");
            characterCustomization.ApplyCharacterVars(characterData);
            Debug.Log("Character appearance set complete");
        }
    }
}
