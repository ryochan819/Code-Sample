using UnityEngine;

namespace Gacha.ui
{
    public class GameSceneInfaceState_Build : IGameSceneInfaceState
    {
        private GameSceneInterfaceManager manager;

        public void Enter(GameSceneInterfaceManager manager)
        {
            this.manager = manager;

            var mainCanvas = manager.UICanvases.mainCanvas;

            if (mainCanvas != null && !mainCanvas.activeSelf)
            {
                mainCanvas.SetActive(true);
            }

            manager.UICanvases.buildCanvas.SetActive(true);
        }

        public void UpdateUI(GameSceneInterfaceStateData stateData)
        {
            
        }

        public void Exit()
        {
            manager.UICanvases.buildCanvas.SetActive(false);
        }
    }
}
