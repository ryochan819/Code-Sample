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
            base.handleInput();

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                controller.SwitchState(controller.PhoneState);
                UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.phone);
            }
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
