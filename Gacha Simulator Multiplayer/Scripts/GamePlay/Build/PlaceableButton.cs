using System;
using Gacha.gameplay;
using Gacha.system;
using UnityEngine;
using UnityEngine.UI;

namespace Gacha.ui
{
    public class PlaceableButton : MonoBehaviour
    {
        [SerializeField] BuildScriptable buildScriptable;
        [SerializeField] Button button;

        void Start()
        {
            button.onClick.AddListener(HandleBuild);
        }

        private void HandleBuild()
        {
            if (buildScriptable == null) return;

            if (buildScriptable.category != Category.prize)
            {
                if (buildScriptable.cost < GameSceneDataManager.instance.money.value)
                {
                    // UI warning
                    return;
                }
            }

            GameSceneDataManager.instance.LocalPlayer.BuildManager.StartBuildMode(buildScriptable);
        }

        void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
