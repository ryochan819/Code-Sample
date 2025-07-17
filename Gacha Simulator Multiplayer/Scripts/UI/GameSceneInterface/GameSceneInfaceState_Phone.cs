using PrimeTween;
using UnityEngine;

namespace Gacha.ui
{
    public class GameSceneInfaceState_Phone : IGameSceneInfaceState
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

            manager.UICanvases.phoneCanvas.SetActive(true);

            RectTransform rt = manager.PhoneFrame.GetComponent<RectTransform>();

            Tween.UIAnchoredPosition(rt,
            startValue: new Vector2(rt.anchoredPosition.x, -300),
            endValue: new Vector2(rt.anchoredPosition.x, 300),
            duration: 0.5f, ease: Ease.OutBack);
        }

        public void UpdateUI(GameSceneInterfaceStateData stateData)
        {
            
        }

        public void Exit()
        {
            RectTransform rt = manager.PhoneFrame.GetComponent<RectTransform>();

            Tween.UIAnchoredPosition(rt,
            startValue: new Vector2(rt.anchoredPosition.x, 300),
            endValue: new Vector2(rt.anchoredPosition.x, -300),
            duration: 0.5f, ease: Ease.OutBack)
            .OnComplete(() => manager.UICanvases.phoneCanvas.SetActive(false));
        }
    }
}
