using System;
using Gacha.gameplay;
using UnityEngine;

namespace Gacha.ui
{
    public class LoadingCanvas : MonoBehaviour
    {
        [SerializeField] GameObject loadingScreen;
        private void OnGameSetupComplete()
        {
            loadingScreen.SetActive(false);
        }

        void OnEnable()
        {
            loadingScreen.SetActive(true);
            GameEventSystem.OnGameSetupComplete += OnGameSetupComplete;
        }

        void OnDisable()
        {
            GameEventSystem.OnGameSetupComplete -= OnGameSetupComplete;
        }
    }
}