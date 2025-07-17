using System;
using CC;
using Gacha.system;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;

namespace Gacha.gameplay
{
    public class PlayerController : NetworkBehaviour
    {
        private PlayerControllerState currentState;

        [Header("Controller States")]
        private PlayerControllerState idleState;
        public PlayerControllerState IdleState => idleState;
        private PlayerControllerState phoneState;
        public PlayerControllerState PhoneState => phoneState;
        private PlayerControllerState buildState;
        public PlayerControllerState BuildState => buildState;
        private PlayerControllerState gachaState;
        public PlayerControllerState GachaState => gachaState;

        [Header("Base setup")]
        public float walkingSpeed = 7.5f;
        public float runningSpeed = 11.5f;
        public float jumpSpeed = 2.0f;
        public float gravity = 20.0f;
        public float lookSpeed = 2.0f;
        public float lookXLimit = 45.0f;
        CharacterController characterController;
        public CharacterController CharacterController => characterController;

        [HideInInspector]
        public bool canMove = true;

        [Header("Crouch setup")]
        bool isCrouching = false;
        public bool IsCrouching { get => isCrouching; set => isCrouching = value; }
        float crouchHeight = 0.9f;
        public float CrouchHeight => crouchHeight;
        float standingHeight = 2.0f;
        public float StandingHeight => standingHeight;

        [SerializeField] BuildManager buildManager;
        public BuildManager BuildManager => buildManager;
        [SerializeField] InteractManager interactManager;
        public InteractManager InteractManager => interactManager;
        [SerializeField] Animator animator;
        public Animator Animator => animator;
        [SerializeField] private float cameraYOffset = 1.75f;
        [SerializeField] Transform eyeLevel;
        [SerializeField] CinemachineCamera playerCamera;
        public CinemachineCamera PlayerCamera => playerCamera;
        [SerializeField] NetworkAnimator networkAnimator;
        public NetworkAnimator NetAnimator => networkAnimator;
        [SerializeField] CharacterCustomization characterCustomization;

        void Awake()
        {
            idleState ??= new PlayerControllerState_Idle(this);
            phoneState ??= new PlayerControllerState_Phone(this);
            buildState ??= new PlayerControllerState_Building(this);
            gachaState ??= new PlayerControllerState_Gacha(this);
        }

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned();

            if (asServer) return;

            if (!isOwner)
            {
                enabled = false;
                return;
            }

            GameSceneDataManager.instance.SetLocalPlayer(this);

            characterController = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SwitchState(idleState);
            playerCamera.enabled = true;
            characterController.enabled = true;
        }

        void Update()
        {
            currentState?.handleInput();
        }

        void FixedUpdate()
        {
            currentState?.handleMovement();
        }

        private void SwitchPlayerState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.idle:
                    SwitchState(IdleState);
                    break;
                case PlayerState.phone:
                    SwitchState(PhoneState);
                    break;
                case PlayerState.build:
                    SwitchState(BuildState);
                    break;
                case PlayerState.gacha:
                    SwitchState(GachaState);
                    break;
                default:
                    Debug.LogWarning($"Unknown player state: {state}");
                    break;
            }
        }

        public void SwitchState(PlayerControllerState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        private void OnCharacterCustomizationLoaded(CharacterCustomization script)
        {
            if (!isOwner)
            {
                return;
            }
            
            standingHeight = eyeLevel.position.y - gameObject.transform.position.y;
            playerCamera.transform.localPosition = new Vector3(0f, standingHeight, 0.1f);
        }

        void OnEnable()
        {
            GameEventSystem.onSwitchPlayerState += SwitchPlayerState;
            characterCustomization.onCharacterLoaded += OnCharacterCustomizationLoaded;
        }

        void OnDisable()
        {
            GameEventSystem.onSwitchPlayerState -= SwitchPlayerState;
            characterCustomization.onCharacterLoaded -= OnCharacterCustomizationLoaded;
        }
    }

    public enum PlayerState
    {
        idle,
        phone,
        build,
        gacha
    }
}
