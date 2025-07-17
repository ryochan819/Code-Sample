using System;
using Gacha.gameplay;
using UnityEngine;

namespace Gacha.ui
{
    public static class UIEventSystem
    {
        public static event Action<IGameSceneInfaceState> onChangeUIInterfaceState;
        public static void ChangeUIInterfaceState(IGameSceneInfaceState state)
        {
            onChangeUIInterfaceState?.Invoke(state);
        }

        public static event Action<GameSceneInterfaceStateData> onUpdateUI;
        public static void UpdateUI(GameSceneInterfaceStateData uiData)
        {
            onUpdateUI?.Invoke(uiData);
        }

        public static event Action<CommonUIData> onUpdateCommonUI;
        public static void UpdateCommonUI(CommonUIData uiData)
        {
            onUpdateCommonUI?.Invoke(uiData);
        }

        public static event Action onClosePhoneMenu;
        public static void ClosePhoneMenu()
        {
            onClosePhoneMenu?.Invoke();
        }

        public static Action<InteractableTypeList> onTargetingIInteractable;
        public static void SwitchInteractableUI(InteractableTypeList interactableType)
        {
            onTargetingIInteractable?.Invoke(interactableType);
        }

        public static event Action<float> SetRightClickHoldRing;
        public static void SetRightClickHoldRingEvent(float value)
        {
            SetRightClickHoldRing?.Invoke(value);
        }
        
        public static event Action<bool> onCenterDotToggle;
        public static void SetCenterDotToggle(bool value)
        {
            onCenterDotToggle?.Invoke(value);
        }
    }
}
