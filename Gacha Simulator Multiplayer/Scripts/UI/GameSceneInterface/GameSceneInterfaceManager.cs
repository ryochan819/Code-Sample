using System;
using TMPro;
using UnityEngine;

namespace Gacha.ui
{
    public class GameSceneInterfaceManager : MonoBehaviour
    {
        public IGameSceneInfaceState currentState;
        
        [SerializeField] private InterfaceCanvases uiCanvases;
        public InterfaceCanvases UICanvases => uiCanvases;

        [Header("CommonCanvasUI")]
        [SerializeField] TextMeshProUGUI moneyText;
        public TextMeshProUGUI MoneyText => moneyText;

        [Header("PhoneCanvasUI")]
        [SerializeField] GameObject phoneFrame;
        public GameObject PhoneFrame => phoneFrame;

        public static class GameStates
        {
            public static readonly IGameSceneInfaceState idle = new GameSceneInfaceState_Idle();
            public static readonly IGameSceneInfaceState phone = new GameSceneInfaceState_Phone();
            public static readonly IGameSceneInfaceState build = new GameSceneInfaceState_Build();
        }

        void Awake()
        {
            UIEventSystem.onChangeUIInterfaceState += ChangeState;
            UIEventSystem.onUpdateUI += UpdateUI;
            UIEventSystem.onUpdateCommonUI += UpdateCommonUI;
        }

        public void ChangeState(IGameSceneInfaceState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter(this);
        }

        private void UpdateUI(GameSceneInterfaceStateData data)
        {
            currentState?.UpdateUI(data);
        }

        void UpdateCommonUI(CommonUIData data)
        {
            switch (data.uiTarget)
            {
                case CommonUIData.UITarget.money:
                    moneyText.text = data.money.ToString("N0");
                    break;
            }
        }

        void OnDestroy()
        {
            UIEventSystem.onChangeUIInterfaceState -= ChangeState;
            UIEventSystem.onUpdateUI -= UpdateUI;
            UIEventSystem.onUpdateCommonUI -= UpdateCommonUI;
        }
    }

    [System.Serializable]
    public struct InterfaceCanvases
    {
        public GameObject mainCanvas;
        public GameObject idleCanvas;
        public GameObject phoneCanvas;
        public GameObject buildCanvas;
    }
}