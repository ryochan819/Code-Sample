using Gacha.ui;
using PrimeTween;
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
            if (!BuildManager.Instance.IsPlacing || BuildManager.Instance.CurrentPreview == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, BuildManager.Instance.BuildableSurfaces))
            {
                Tween.Position(BuildManager.Instance.CurrentPreview.transform, hit.point, 0.1f);
            }

            if (Input.GetKeyDown(KeyCode.R))
                BuildManager.Instance.CurrentPreview.transform.Rotate(0, 90, 0);

            if (Input.GetMouseButtonDown(0))
                BuildManager.Instance.ConfirmPlacement();

            if (Input.GetMouseButtonDown(1))
                BuildManager.Instance.LeaveBuildMode();
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
