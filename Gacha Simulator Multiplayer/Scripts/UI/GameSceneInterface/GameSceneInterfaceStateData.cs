using UnityEngine;

namespace Gacha.ui
{
    public abstract class GameSceneInterfaceStateData { }

    public class CommonUIData : GameSceneInterfaceStateData
    {
        public enum UITarget
        {
            money
        }
        public UITarget uiTarget;
        public int money;
    }

    public class IdleUIData : GameSceneInterfaceStateData
    {
        public string message;
    }

    public class ErrorUIData : GameSceneInterfaceStateData
    {
        public string message;
    }
}