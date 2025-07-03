using UnityEngine;

namespace Gacha.gameplay
{
    public class PrizeData : ScriptableObject
    {
        public string setName;
        public bool isMod;
        public string modID; // Optional
        public Texture thumbnailImage;
        public int stockPrice;
    }

    [System.Serializable]
    public class DropChance
    {
        public RareType rareType;
        [Range(0f, 1f)]
        public float dropChance;

        public DropChance(RareType type, float chance)
        {
            rareType = type;
            dropChance = chance;
        }
    }

    public enum RareType
    {
        Common,
        Uncommon,
        Rare,
        SuperRare,
        UltraRare,
        Legendary
    }
}
