using Gacha.ui;
using UnityEngine;

namespace Gacha.gameplay
{
    public class InteractManager : MonoBehaviour
    {
        private IInteractableWithCancel currentInteractableWithCancel;
        public void SetCurrentInteractingInteractable(IInteractableWithCancel interactable)
        {
            currentInteractableWithCancel = interactable;
        }

        public void Reset()
        {
            currentInteractableWithCancel.CancelInteraction();
            currentInteractableWithCancel = null;

            GameEventSystem.SwitchPlayerState(PlayerState.idle);
            UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.idle);
        }
    }
}
