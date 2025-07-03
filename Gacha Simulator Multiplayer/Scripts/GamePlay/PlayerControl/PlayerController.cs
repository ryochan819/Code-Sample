using System;
using CC;
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
        [SerializeField] Animator animator;
        public Animator Animator => animator;
        [SerializeField] private float cameraYOffset = 1.75f;
        [SerializeField] Transform eyeLevel;
        [SerializeField] CinemachineCamera playerCamera;
        public CinemachineCamera PlayerCamera => playerCamera;
        [SerializeField] NetworkAnimator newWorkAnimator;
        [SerializeField] CharacterCustomization characterCustomization;
        void Awake()
        {
            idleState = new PlayerControllerState_Idle(this);
            phoneState = new PlayerControllerState_Phone(this);
            buildState = new PlayerControllerState_Building(this);
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
            }
        }

        public void SwitchState(PlayerControllerState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        private void OnCharacterLoaded(CharacterCustomization script)
        {
            playerCamera.transform.localPosition = new Vector3(0f, eyeLevel.position.y - gameObject.transform.position.y, 0.1f);
        }

        void OnEnable()
        {
            GameEventSystem.onSwitchPlayerState += SwitchPlayerState;
            if (characterCustomization)
                characterCustomization.onCharacterLoaded += OnCharacterLoaded;
        }

        void OnDisable()
        {
            GameEventSystem.onSwitchPlayerState -= SwitchPlayerState;
            if (characterCustomization)
                characterCustomization.onCharacterLoaded -= OnCharacterLoaded;
        }
    }

    public enum PlayerState
    {
        idle,
        phone,
        build
    }
}
