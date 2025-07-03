using UnityEngine;

namespace Gacha.gameplay
{
    [CreateAssetMenu(menuName = "Prize Data/Capsule Toy Set Data")]
    public class CapsuleToySetData : PrizeData
    {
        public int drawPrice_InCoins; // in coins
        public bool dropRateEqual = true; // true = all equal, false = specific
        public DropChance[] dropChances; // Used only if dropRateEqual == false
        public CapsuleSize capsuleSize; // Size of the capsule toy
        public CapsuleToyEntry[] capsuleToys;
    }

    [System.Serializable]
    public class CapsuleToyEntry
    {
        public string toyName; // Optional: For easier identification in inspector
        public Sprite toyImage; // 1 : 1 aspect ratio
        public GameObject toyPrefab;
        public RareType toyRareType; // Used only if dropRateEqual == false

        public CapsuleToyEntry(string name, Sprite image, GameObject prefab, RareType rareType)
        {
            toyName = name;
            toyImage = image;
            toyPrefab = prefab;
            toyRareType = rareType;
        }
    }

    public enum CapsuleSize
    {
        Size48mm,
        Size65mm
    }
}
