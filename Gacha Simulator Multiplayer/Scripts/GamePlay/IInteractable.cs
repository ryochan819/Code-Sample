using UnityEngine;

namespace Gacha.gameplay
{
    public interface IInteractable
    {
        public InteractableTypeList InteractableType { get; }
        bool Interact();
        void EnableOutline(bool enable);
    }

    public interface IInteractableWithCancel : IInteractable
    {
        void CancelInteraction();
    }

    public enum InteractableTypeList
    {
        None,
        GachaMachine,
        CoinInsert,
        GachaHandle
    }
}
