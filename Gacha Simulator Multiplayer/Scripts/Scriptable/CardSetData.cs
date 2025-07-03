using UnityEngine;

namespace Gacha.gameplay
{
    [CreateAssetMenu(menuName = "Prize Data/Card Set Data")]
    public class CardSetData : PrizeData
    {
        public CardPackType packType;
        public bool dropRateEqual;
        public Sprite packImage;
        public CardEntry[] cardEntries;
    }

    [System.Serializable]
    public class CardEntry
    {
        public string cardName; // Optional: For easier identification in inspector
        public Sprite cardImage;
        public CardPackTexture cardTexture;
        public DropChance dropChance; // Used only if dropRateEqual == false

        public CardEntry(string name, Sprite image, CardPackTexture texture, DropChance chance)
        {
            cardName = name;
            cardImage = image;
            cardTexture = texture;
            dropChance = chance;
        }
    }

    public enum CardPackType
    {
        StandardPack,
        Box
    }

    public enum CardPackTexture
    {
        Standard,
        Silver,
        Gold,
        Platinum,
        Limited
    }
}
