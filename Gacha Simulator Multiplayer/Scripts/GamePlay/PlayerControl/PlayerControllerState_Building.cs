using PrimeTween;
using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public class PlayerControllerState_Building : PlayerControllerState
    {
        public PlayerControllerState_Building(PlayerController controller) : base(controller)
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
            base.handleInput();

            HandleBuilding();
        }

        public void HandleBuilding()
        {
            if (!controller.BuildManager.IsPlacing || controller.BuildManager.CurrentPreview == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, controller.BuildManager.BuildableSurfaces))
            {
                controller.BuildManager.UpdatePreviewObjectPosition(hit);
            }

            if (Input.GetKeyDown(KeyCode.R))
                controller.BuildManager.CurrentPreview.transform.Rotate(0, 90, 0);

            if (Input.GetMouseButtonDown(0))
                controller.BuildManager.ConfirmPlacement();

            if (Input.GetMouseButtonDown(1))
                controller.BuildManager.LeaveBuildMode();
                
            float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (controller.BuildManager.CurrentPreview != null)
            {
                if (mouseScrollWheel < 0)
                {
                    controller.BuildManager.AdjustPlacementRotation(15);
                }
                else if (mouseScrollWheel > 0)
                {
                    controller.BuildManager.AdjustPlacementRotation(-15);
                }
            }
        }

        public override void handleMovement()
        {
            base.handleMovement();
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
