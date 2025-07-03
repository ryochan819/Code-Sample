using System;
using Gacha.system;
using UnityEngine;
using PurrNet;

namespace Gacha.gameplay
{
    public class GachaMachine : NetworkBehaviour
    {
        [Header("Machine Setting")]
        CapsuleToySetData capsuleToySetOnMachine;
        int currentToyInsideMachine = 0;
        int currentCoins;
        int requiredCoins = 5;
        [SerializeField] Transform capsuleSpawnPoint;
        [SerializeField] GameObject coverPoster;

        [Header("Npc Interaction")]
        bool isNPCInteracting;
        public bool IsNPCInteracting
        {
            get { return isNPCInteracting; }
            set { isNPCInteracting = value; }
        }

        [Header("Special Rules")]
        public bool isMenu = false;

        public event Action OnCapsuleSpawned;

        public void SetCapsuleMachineToySet(CapsuleToySetData capsuleToySet)
        {
            if (capsuleToySet == null)
            {
                Debug.LogError("CapsuleToySetData is null. Cannot set toy set.");
                return;
            }

            capsuleToySetOnMachine = capsuleToySet;

            requiredCoins = capsuleToySet.drawPrice_InCoins;

            Texture coverImage = capsuleToySet.thumbnailImage;
            if (coverImage == null)
            {
                // pending show default image
            }
            MeshRenderer meshRenderer = coverPoster.GetComponent<MeshRenderer>();
            var block = new MaterialPropertyBlock();
            block.SetTexture("_BaseMap", coverImage);

            meshRenderer.SetPropertyBlock(block);
        }

        public bool EnoughCoins()
        {
            return currentCoins >= requiredCoins;
        }

        public void AddCurrentCoins()
        {
            currentCoins += 1;
        }

        public bool HasToy()
        {
            return currentToyInsideMachine > 0;
        }

        public void SpawnCapsule()
        {
            if (capsuleToySetOnMachine != null)
            {
                GameObject capsulePrefab = GameReference.Instance.GetCapsuleBySize(capsuleToySetOnMachine.capsuleSize);

                Quaternion randomRotation = UnityEngine.Random.rotation;
                GameObject capsuleSpawn = Instantiate(capsulePrefab, capsuleSpawnPoint.position, randomRotation);
                CapsuleController capsuleController = capsuleSpawn.GetComponent<CapsuleController>();
                capsuleController.SetCapsuleData(capsuleToySetOnMachine);

                if (!isMenu)
                {
                    currentToyInsideMachine--;
                }
            }
        }

        public void Reset()
        {
            currentCoins = 0;
        }
    }
}