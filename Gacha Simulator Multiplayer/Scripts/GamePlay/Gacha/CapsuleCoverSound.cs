using Gacha.gameplay;
using UnityEngine;

public class CapsuleCoverSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    void OnCollisionEnter(Collision collision)
    {
        AudioClip capsuleLandSound = SoundManager.Instance.GetClip(SoundType.CapsuleLand);
        audioSource.PlayOneShot(capsuleLandSound);
    }
}
