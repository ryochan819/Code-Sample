using System;
using UnityEngine;

namespace Gacha.gameplay
{
    public class PlayerControllerState_Gacha : PlayerControllerState
    {
        public PlayerControllerState_Gacha(PlayerController controller) : base(controller)
        {
            this.controller = controller;
        }
        
        public override void EnterState()
        {
            base.EnterState();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void handleInput()
        {
            HighlightInteractableObjects();

            HandleLeftClick();

            bool dragging = HandleDragging();

            HandleLeftMouseUp();

            if (!dragging)
            {
                HandleRightMouseUp();
            }
        }

        private void HighlightInteractableObjects()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("Interactable", "Capsule");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject != rayCastObject)
                {
                    // Disable previous outline
                    if (rayCastObject != null)
                    {
                        Outline outline = rayCastObject.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = false;
                        }
                    }

                    // Enable new outline
                    if (hitObject.CompareTag("GachaSwitch") || hitObject.CompareTag("CoinInsert") || hitObject.CompareTag("Capsule"))
                    {
                        Outline outline = hitObject.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = true;
                        }
                        rayCastObject = hitObject;

                        Debug.Log("GachaSwitch or CoinInsert or Capsule detected: " + hitObject.name);
                    }
                    else
                    {
                        rayCastObject = null;
                    }
                }
            }
            else
            {
                if (rayCastObject != null)
                {
                    rayCastObject.GetComponent<Outline>().enabled = false;
                    rayCastObject = null;
                }
            }
        }

        private void HandleLeftClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                LayerMask layerMask = LayerMask.GetMask("Interactable", "Capsule");

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
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
                    
                    if (hit.collider.CompareTag("Capsule"))
                    {
                        CapsuleController capsuleController = hit.transform.GetComponent<CapsuleController>();

                        if (capsuleController.CapsuleStatus == CapsuleStatus.InsideMachine)
                        {
                            capsuleController.SetCapsuleStatus(CapsuleStatus.WaitingToOpenOnMenu);
                            // StartCoroutine(Utility.BounceToPosition(hit.transform, capsuleTarget.position, capsuleTarget.rotation, 0.5f, 0.1f));
                            // handleCollider.enabled = false;
                            // coinInsertCollider.enabled = false;
                            Debug.Log("Capsule clicked: " + hit.collider.name);
                            return;
                        }

                        if (capsuleController.CapsuleStatus == CapsuleStatus.WaitingToOpenOnMenu)
                        {
                            // currentCapsuleController = capsuleController.OpenCapsule();
                            // uiBlockerCapsuleText.text = currentCapsuleController.Item1.toyName;
                            // string rareText = currentCapsuleController.Item1.toyRareType.ToString();
                            // uiBlockerRarityText.text = "<link=" + rareText + ">" + rareText + "</link>";
                            // uiBlocker.SetActive(true);
                        }
                    }
                }
                else
                {
                    selectedObject = null;
                }
            }
        }

        private bool HandleDragging()
        {
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
                        selectedObject.transform.Rotate(0, 0, -angleDifference);
                        bool rotateComplete = gachaHandle.HandleRotated();
                        if (rotateComplete)
                        {
                            isDragging = false;
                            gachaHandle.ResetHandle(false);
                            return false;
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

                        selectedObject.transform.Rotate(0, 0, -rotationToApply);
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

            return true;
        }

        private void HandleLeftMouseUp()
        {
            if (Input.GetMouseButtonUp(0))
            {
                lastMousePosition = Input.mousePosition; // Reset to prevent jumps
                isDragging = false;
                selectedObject = null;
                rayCastObject = null;
                isEnoughCoins = false;
            }
        }

        private void HandleRightMouseUp()
        {
            if (Input.GetMouseButtonUp(1))
            {
                controller.InteractManager.Reset();
            }
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
