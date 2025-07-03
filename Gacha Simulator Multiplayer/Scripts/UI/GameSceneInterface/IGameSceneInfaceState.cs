using UnityEngine;

namespace Gacha.ui
{
    public interface IGameSceneInfaceState
    {
        void Enter(GameSceneInterfaceManager manager);
        void UpdateUI(GameSceneInterfaceStateData stateData);
        void Exit();
    }
}