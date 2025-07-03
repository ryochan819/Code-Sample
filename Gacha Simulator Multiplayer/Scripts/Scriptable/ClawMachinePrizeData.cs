using UnityEngine;

namespace Gacha.gameplay
{
    [CreateAssetMenu(menuName = "Prize Data/Claw Machine Prize Data")]
    public class ClawMachinePrizeData : PrizeData
    {
        public int drawPrice;
        public ClawMachineType clawMachineType;
        public PlacementType placementType;
        public ClawMachineToyEntry[] toyPrefabs;
    }

    [System.Serializable]
    public class ClawMachineToyEntry
    {
        public string toyName; // Optional: For easier identification in inspector
        public Sprite toyImage;
        public GameObject toyPrefab;

        public ClawMachineToyEntry(string name, Sprite image, GameObject prefab)
        {
            toyName = name;
            toyImage = image;
            toyPrefab = prefab;
        }
    }

    public enum ClawMachineType
    {
        TwoClawStandard,
        ThreeClawStandard,
        PinToHole,
        RainbowShoot,
        OneMillion,
        TwoBar
    }

    public enum PlacementType
    {
        Default,
        Standalone,
        Spread,
        Stack
    }
}