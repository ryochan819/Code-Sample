using Gacha.system;
using UnityEngine;
using PurrNet;
using Unity.Cinemachine;
using Gacha.ui;

namespace Gacha.gameplay
{
    public class GachaMachine : NetworkBehaviour, IInteractableWithCancel
    {
        [Header("Machine Setting")]
        CapsuleToySetData capsuleToySetOnMachine;
        public CapsuleToySetData CapsuleToySetOnMachine => capsuleToySetOnMachine;
        int currentToyInsideMachine = 0;
        int currentCoins;
        int requiredCoins = 5;
        [SerializeField] Transform capsuleSpawnPoint;
        [SerializeField] GameObject coverPoster;
        int machineGroundLevel = 0;
        bool machineTopOccupied = false;
        [SerializeField] Transform topSnapPosition;
        public Transform TopSnapPosition => topSnapPosition;
        public InteractableTypeList InteractableType => InteractableTypeList.GachaMachine;
        Outline outline;

        [Header("Player Interaction")]
        [SerializeField] CinemachineCamera gachaCineCam;
        [SerializeField] GameObject tunnelCollider;
        [SerializeField] Collider handleCollider;
        [SerializeField] Collider coinInsertCollider;

        [Header("Npc Interaction")]
        SyncVar<bool> isInteracting = new(false);
        public bool IsInteracting
        {
            get { return isInteracting.value; }
            set { isInteracting.value = value; }
        }

        [Header("Special Rules")]
        public bool isMenu = false;

        public bool Interact()
        {
            if (!isInteracting.value)
            {
                // Check if player is in front of the machine
                Vector3 toPlayer = (GameSceneDataManager.instance.LocalPlayer.transform.position - gameObject.transform.position).normalized;
                float dot = Vector3.Dot(gameObject.transform.forward, toPlayer);

                if (dot < 0f)
                {
                    // Show warning
                    Debug.Log("Player is behind the object.");
                    return false;
                }

                isInteracting.value = true;

                GameEventSystem.CameraBlendUpdate(BlendMode.EaseInOut, 0.5f);

                gachaCineCam.enabled = true;
                gachaCineCam.Priority = 10;
                tunnelCollider.SetActive(true);
                handleCollider.enabled = true;
                coinInsertCollider.enabled = true;
                EnableOutline(false);

                GameSceneDataManager.instance.LocalPlayer.InteractManager.SetCurrentInteractingInteractable(this);
                
                GameEventSystem.SwitchPlayerState(PlayerState.gacha);
                UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.gacha);

                return true;
            }
            return false;
        }

        public void CancelInteraction()
        {
            isInteracting.value = false;

            gachaCineCam.Priority = 0;
            gachaCineCam.enabled = false;
            tunnelCollider.SetActive(false);
            handleCollider.enabled = false;
            coinInsertCollider.enabled = false;
        }

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

        public bool CanSnapOnTop()
        {
            return !machineTopOccupied && machineGroundLevel < 2;
        }

        public int GetMachineGroundLevel()
        {
            return machineGroundLevel;
        }

        public void Reset()
        {
            currentCoins = 0;
        }

        public void EnableOutline(bool enable)
        {
            if (outline == null)
                outline = GetComponent<Outline>();

            if (outline != null && outline.enabled != enable)
                outline.enabled = enable;
        }
    }
}