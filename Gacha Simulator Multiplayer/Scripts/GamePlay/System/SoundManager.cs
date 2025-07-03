using UnityEngine;

namespace Gacha.gameplay
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio")]
        [SerializeField] AudioClip gachaRotateSound;
        [SerializeField] AudioClip gachaFailSound;
        [SerializeField] AudioClip[] insertCoinSounds;
        [SerializeField] AudioClip[] shiningSounds;
        [SerializeField] AudioClip[] capsuleLandSounds;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public AudioClip GetClip(SoundType type)
        {
            switch (type)
            {
                case SoundType.GachaRotate:
                    return gachaRotateSound;
                case SoundType.GachaFail:
                    return gachaFailSound;
                case SoundType.InsertCoinRandom:
                    return GetRandomInsertCoinSound();
                case SoundType.CapsuleLand:
                    return capsuleLandSounds[Random.Range(0, capsuleLandSounds.Length)];
                default:
                    return null;
            }
        }

        public AudioClip GetShiningSound(RareType type)
        {
            return shiningSounds[(int)type];
        }

        public AudioClip GetRandomInsertCoinSound()
        {
            if (insertCoinSounds == null || insertCoinSounds.Length == 0)
                return null;

            int index = Random.Range(0, insertCoinSounds.Length);
            return insertCoinSounds[index];
        }
    }

    public enum SoundType
    {
        GachaRotate,
        GachaFail,
        InsertCoinRandom,
        CapsuleLand
    }
}