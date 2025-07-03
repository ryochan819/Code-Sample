using UnityEngine;

namespace Gacha.gameplay
{
    public abstract class PlayerControllerState
    {
        protected PlayerController controller;

        // Movement and rotation
        Vector3 moveDirection = Vector3.zero;
        float rotationX = 0;
        private float limitedRotation = 0f;
        private bool reachedLimit = false;

        // Object selection and dragging
        private GameObject selectedObject;
        private bool isDragging = false;
        private Vector3 lastMousePosition;
        private GameObject rayCastObject;

        // Coin and interaction logic
        private bool isEnoughCoins = false;

        // Placement and positioning
        private Vector3 centerPoint;

        public PlayerControllerState(PlayerController controller)
        {
            this.controller = controller;
        }

        public virtual void EnterState()
        {
            Debug.Log("Entering state: " + this.GetType().Name);
        }

        public virtual void ExitState()
        {
            Debug.Log("Exiting state: " + this.GetType().Name);
        }

        public virtual void handleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("GachaSwitch"))
                    {
                        selectedObject = hit.collider.gameObject;
                        centerPoint = Camera.main.WorldToScreenPoint(selectedObject.transform.position);
                        lastMousePosition = Input.mousePosition;
                        isDragging = true;
                        isEnoughCoins = selectedObject.GetComponent<GachaHandle>().GetIsEnoughCoins();
                    }

                    if (hit.collider.CompareTag("CoinInsert"))
                    {
                        bool coinAdded = hit.collider.GetComponent<GachaCoinInsert>().AddCoins();

                        if (coinAdded)
                        {
                            // Do something
                        }
                    }
                }
                else
                {
                    selectedObject = null;
                }
            }

            if (Input.GetMouseButton(0) && isDragging && selectedObject != null && selectedObject.tag == "GachaSwitch")
            {
                Vector2 currMousePosition = Input.mousePosition;

                GachaHandle gachaHandle = selectedObject.GetComponent<GachaHandle>();

                if (isEnoughCoins)
                {
                    Vector2 prevDir = lastMousePosition - centerPoint;
                    Vector2 currDir = currMousePosition - (Vector2)centerPoint;

                    float angleDifference = prevDir.SignedAngleTo(currDir);

                    if (angleDifference < 0) // Only rotate clockwise
                    {
                        selectedObject.transform.Rotate(0, 0, angleDifference);
                        bool rotateComplete = gachaHandle.HandleRotated();
                        if (rotateComplete)
                        {
                            isDragging = false;
                            return;
                        }
                    }

                    lastMousePosition = currMousePosition;
                }
                else if (!reachedLimit)
                {
                    Vector2 prevDir = lastMousePosition - centerPoint;
                    Vector2 currDir = currMousePosition - (Vector2)centerPoint;

                    float angleDifference = prevDir.SignedAngleTo(currDir);

                    if (angleDifference < 0) // Only clockwise
                    {
                        float remaining = 5f - limitedRotation;
                        float rotationToApply = Mathf.Max(angleDifference, -remaining); // angleDifference is negative

                        selectedObject.transform.Rotate(0, 0, rotationToApply);
                        limitedRotation += -rotationToApply; // make positive

                        if (limitedRotation >= 5f)
                        {
                            reachedLimit = true;

                            gachaHandle.ResetHandle();
                        }
                    }

                    lastMousePosition = currMousePosition;
                }
            }

            HandleMouseUp();
        }

        private void HandleMouseUp()
        {
            if (Input.GetMouseButtonUp(0))
            {
                lastMousePosition = Input.mousePosition; // Reset to prevent jumps
                isDragging = false;
                selectedObject = null;
                isEnoughCoins = false;
            }
        }

        public virtual void handleMovement()
        {
            if (!controller.CharacterController) return;

            // Raw input
            float inputVertical = Input.GetAxis("Vertical");
            float inputHorizontal = Input.GetAxis("Horizontal");

            // Movement
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            Vector3 forward = controller.transform.TransformDirection(Vector3.forward);
            Vector3 right = controller.transform.TransformDirection(Vector3.right);

            float moveSpeed = controller.canMove ? (isRunning ? controller.runningSpeed : controller.walkingSpeed) : 0f;
            float curSpeedX = moveSpeed * inputVertical;
            float curSpeedY = moveSpeed * inputHorizontal;
            float movementDirectionY = moveDirection.y;

            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButtonDown("Jump") && controller.canMove && controller.CharacterController.isGrounded)
            {
                moveDirection.y = controller.jumpSpeed;
            }
            else
            {
                moveDirection.y = movementDirectionY;
            }

            if (!controller.CharacterController.isGrounded)
            {
                moveDirection.y -= controller.gravity * Time.deltaTime;
            }

            controller.CharacterController.Move(moveDirection * Time.deltaTime);

            // Camera
            if (controller.canMove && controller.PlayerCamera)
            {
                rotationX += -Input.GetAxis("Mouse Y") * controller.lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -controller.lookXLimit, controller.lookXLimit);

                controller.PlayerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                controller.transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * controller.lookSpeed, 0);
            }

            // Animation input smoothing (for blend trees)
            if (controller.Animator)
            {
                Vector2 rawInput = new Vector2(inputHorizontal, inputVertical);
                Vector2 normalizedInput = rawInput.normalized;

                controller.Animator.SetFloat("horizontal", controller.canMove ? normalizedInput.x : 0f, 0.1f, Time.deltaTime);
                controller.Animator.SetFloat("vertical", controller.canMove ? normalizedInput.y : 0f, 0.1f, Time.deltaTime);

                // Jumping state
                bool isJumping = !controller.CharacterController.isGrounded;
                if (controller.Animator.GetBool("Jumping") != isJumping)
                {
                    controller.Animator.SetBool("Jumping", isJumping);
                }
            }
        }
    }
}
