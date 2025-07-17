using PrimeTween;
using UnityEngine;

namespace Gacha.gameplay
{
    public abstract class PlayerControllerState
    {
        protected PlayerController controller;

        // Raycast Detection Range
        internal float detectionRange = 3f;

        // Movement and rotation
        Vector3 moveDirection = Vector3.zero;
        float rotationX = 0;

        // Gacha rotation
        internal float limitedRotation = 0f;
        internal bool reachedLimit = false;

        // Object selection and Gacha handle dragging
        internal GameObject selectedObject;
        internal GameObject rayCastObject;
        internal bool isDragging = false;
        internal Vector3 lastMousePosition;

        // Current Raycasted interactable
        internal IInteractable currentInteractable;

        // Coin and interaction logic
        internal bool isEnoughCoins = false;

        // Placement and positioning
        internal Vector3 centerPoint;

        public PlayerControllerState(PlayerController controller)
        {
            this.controller = controller;
        }

        public virtual void EnterState()
        {
            Debug.Log("Entering state: " + this.GetType().Name);

            if (controller.PlayerCamera != null)
            {
                rotationX = controller.PlayerCamera.transform.localEulerAngles.x;
                if (rotationX > 180)
                    rotationX -= 360;
            }
        }

        public virtual void ExitState()
        {
            Debug.Log("Exiting state: " + this.GetType().Name);
        }

        public virtual void handleInput()
        {
        }

        public virtual void handleMovement()
        {
            if (!controller.CharacterController) return;

            HandleMovementInput(out float inputVertical, out float inputHorizontal, out Vector3 forward, out Vector3 right, out float moveSpeed);
            ApplyMovement(inputVertical, inputHorizontal, forward, right, moveSpeed);
            HandleJumpAndGravity();
            HandleCrouch();
            HandleCameraLook();
            HandleAnimation(inputHorizontal, inputVertical);
        }

        private void HandleMovementInput(out float inputVertical, out float inputHorizontal, out Vector3 forward, out Vector3 right, out float moveSpeed)
        {
            inputVertical = Input.GetAxis("Vertical");
            inputHorizontal = Input.GetAxis("Horizontal");
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            forward = controller.transform.TransformDirection(Vector3.forward);
            right = controller.transform.TransformDirection(Vector3.right);

            moveSpeed = controller.canMove ? (isRunning ? controller.runningSpeed : controller.walkingSpeed) : 0f;
        }

        private void ApplyMovement(float inputVertical, float inputHorizontal, Vector3 forward, Vector3 right, float moveSpeed)
        {
            float curSpeedX = moveSpeed * inputVertical;
            float curSpeedY = moveSpeed * inputHorizontal;
            float movementDirectionY = moveDirection.y;

            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
            moveDirection.y = movementDirectionY;
            
            controller.CharacterController.Move(moveDirection * Time.deltaTime);
        }

        private void HandleJumpAndGravity()
        {
            if (Input.GetButtonDown("Jump") && controller.canMove && controller.CharacterController.isGrounded)
            {
                moveDirection.y = controller.jumpSpeed;
            }

            if (!controller.CharacterController.isGrounded)
            {
                moveDirection.y -= 0.7f * controller.gravity * Time.deltaTime;
            }
        }

        private void HandleCrouch()
        {
            bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl);
            float crouchSpeed = 0.3f;

            if (isCtrlHeld && !controller.IsCrouching)
            {
                controller.IsCrouching = true;
                Tween.LocalPosition(controller.PlayerCamera.transform, new Vector3(0, controller.CrouchHeight, 0.1f), crouchSpeed);

                float startWeight = controller.NetAnimator.GetLayerWeight(3);
                Tween.Custom(startWeight, 1f, crouchSpeed, value =>
                {
                    controller.NetAnimator.SetLayerWeight(3, value);
                });
            }
            else if (!isCtrlHeld && controller.IsCrouching)
            {
                controller.IsCrouching = false;
                Tween.LocalPosition(controller.PlayerCamera.transform, new Vector3(0, controller.StandingHeight, 0.1f), crouchSpeed);

                float startWeight = controller.NetAnimator.GetLayerWeight(3);
                Tween.Custom(startWeight, 0f, crouchSpeed, value =>
                {
                    controller.NetAnimator.SetLayerWeight(3, value);
                });
            }
        }

        private void HandleCameraLook()
        {
            if (!controller.canMove || controller.PlayerCamera == null) return;

            rotationX += -Input.GetAxis("Mouse Y") * controller.lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -controller.lookXLimit, controller.lookXLimit);

            controller.PlayerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            controller.transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * controller.lookSpeed, 0);
        }

        private void HandleAnimation(float inputHorizontal, float inputVertical)
        {
            if (!controller.Animator) return;

            Vector2 rawInput = new Vector2(inputHorizontal, inputVertical);
            Vector2 normalizedInput = rawInput.normalized;

            controller.Animator.SetFloat("horizontal", controller.canMove ? normalizedInput.x : 0f, 0.1f, Time.deltaTime);
            controller.Animator.SetFloat("vertical", controller.canMove ? normalizedInput.y : 0f, 0.1f, Time.deltaTime);

            bool isJumping = !controller.CharacterController.isGrounded;
            if (controller.Animator.GetBool("Jumping") != isJumping)
            {
                controller.Animator.SetBool("Jumping", isJumping);
            }
        }
    }
}
