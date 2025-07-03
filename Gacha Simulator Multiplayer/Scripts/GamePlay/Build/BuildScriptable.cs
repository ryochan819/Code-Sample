using UnityEngine;

namespace Gacha.gameplay
{
    [CreateAssetMenu(fileName = "Buildable", menuName = "Scriptable Objects/Buildable")]
    public class BuildScriptable : ScriptableObject
    {
        public string itemName;
        public Sprite buildImage;
        public GameObject prefab;
        public int unlockLevel;
        public int cost;
        public Category category;
    }

    public enum Category
    {
        capsuleMachine,
        clawMachine,
        decoration,
        prize
    }
}
