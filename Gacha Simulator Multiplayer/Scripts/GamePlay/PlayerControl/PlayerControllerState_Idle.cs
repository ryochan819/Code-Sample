using Gacha.system;
using Gacha.ui;
using UnityEngine;

namespace Gacha.gameplay
{
    public class PlayerControllerState_Idle : PlayerControllerState
    {
        public PlayerControllerState_Idle(PlayerController controller) : base(controller)
        {
            this.controller = controller;
        }

        public override void EnterState()
        {
            base.EnterState();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void handleInput()
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                controller.SwitchState(controller.PhoneState);
                UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.phone);
            }

            // Prioritized layers:
            LayerMask priorityMask = LayerMask.GetMask("GachaMachine");

            // Secondary interactable layers
            // LayerMask interactMask = LayerMask.GetMask("Interactable", "Capsule");

            RaycastHit hit;
            bool raycastFound = Physics.Raycast(controller.PlayerCamera.transform.position, controller.PlayerCamera.transform.forward, out hit, detectionRange, priorityMask);

            // if (!foundPriority)
            // {
            //     foundPriority = Physics.Raycast(controller.PlayerCamera.transform.position, controller.PlayerCamera.transform.forward, out hit, detectionRange, interactMask);
            // }

            if (raycastFound)
            {
                IInteractable interactable = hit.transform.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable != currentInteractable)
                    {
                        currentInteractable?.EnableOutline(false);
                        currentInteractable = interactable;

                        UIEventSystem.SwitchInteractableUI(interactable.InteractableType);
                        interactable.EnableOutline(true);
                    }

                    HandleInteractableInput(interactable);
                }
                else
                {
                    UIEventSystem.SwitchInteractableUI(InteractableTypeList.None);
                    currentInteractable?.EnableOutline(false);
                    currentInteractable = null;
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    UIEventSystem.SwitchInteractableUI(InteractableTypeList.None);
                    currentInteractable?.EnableOutline(false);
                    currentInteractable = null;
                    UIEventSystem.SetRightClickHoldRingEvent(0);
                }
            }
        }

        public void HandleInteractableInput(IInteractable interactable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                interactable.Interact();
            }
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
