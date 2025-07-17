using UnityEngine;

namespace Gacha.ui
{
    public class GameSceneInfaceState_Gacha : IGameSceneInfaceState
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

            manager.UICanvases.gachaCanvas.SetActive(true);
        }

        public void UpdateUI(GameSceneInterfaceStateData stateData)
        {
            
        }

        public void Exit()
        {
            manager.UICanvases.gachaCanvas.SetActive(false);
        }
    }
}
