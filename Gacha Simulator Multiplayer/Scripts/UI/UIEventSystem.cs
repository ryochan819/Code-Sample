using System;
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
    }
}
