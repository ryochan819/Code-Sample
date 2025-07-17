using Gacha.ui;
using UnityEngine;

namespace Gacha.gameplay
{
    public class PlayerControllerState_Phone : PlayerControllerState
    {
        public PlayerControllerState_Phone(PlayerController controller) : base(controller)
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
            if (Input.GetKeyUp(KeyCode.Tab) || Input.GetMouseButtonUp(1))
            {
                controller.SwitchState(controller.IdleState);
                UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.idle);
            }
        }

        public override void handleMovement()
        {
            
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
