using System.Collections;
using System.Linq;
using Gacha.system;
using UnityEngine;

namespace Gacha.gameplay
{
    public class CapsuleController : MonoBehaviour
    {
        [SerializeField] CapsuleSize capsuleSize = CapsuleSize.Size48mm;
        CapsuleStatus capsuleStatus = CapsuleStatus.InsideMachine;
        public CapsuleStatus CapsuleStatus => capsuleStatus;
        CapsuleToySetData capsuleToySetData;
        [SerializeField] Animation animationPlayer;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip openCapsule;
        float defaultCapsulePrizeYPosition = -0.0258f;

        public void SetCapsuleStatus(CapsuleStatus status)
        {
            capsuleStatus = status;

            if (status == CapsuleStatus.WaitingToOpenOnMenu)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        public void SetCapsuleData(CapsuleToySetData capsuleData)
        {
            capsuleToySetData = capsuleData;
        }

        public (CapsuleToyEntry, GameObject) OpenCapsule()
        {
            if (capsuleToySetData == null || capsuleToySetData.capsuleToys.Length == 0)
            {
                Debug.LogWarning("No toys available in the capsule set.");
                return (null, null);
            }

            Debug.Log("Open capsule");

            // Create an empty parent object
            GameObject prize = Instantiate(GameReference.Instance.CapsuleToyParent);
            prize.transform.SetParent(gameObject.transform);
            prize.transform.localPosition = new Vector3(0, defaultCapsulePrizeYPosition, 0);
            prize.transform.localRotation = Quaternion.identity;
            CapsuleToyEntry capsuleToyOpened = null;

            animationPlayer.Play();
            audioSource.clip = openCapsule;
            audioSource.Play();
            GetComponent<Collider>().enabled = false;

            AudioClip rareSound = null;

            // Check if draw by equal drop rate or by rarity
            if (capsuleToySetData.dropRateEqual)
            {
                // Draw a random toy from the set
                int randomIndex = Random.Range(0, capsuleToySetData.capsuleToys.Length);
                capsuleToyOpened = capsuleToySetData.capsuleToys[randomIndex];

                Instantiate(capsuleToyOpened.toyPrefab, prize.transform.position, prize.transform.rotation, prize.transform);

                rareSound = SoundManager.Instance.GetShiningSound(capsuleToySetData.capsuleToys[randomIndex].toyRareType);
                Debug.Log($"Opened a toy: {capsuleToyOpened.toyName}");
            }
            else
            {
                float totalChance = capsuleToySetData.dropChances.Sum(dc => dc.dropChance);
                float roll = Random.Range(0f, totalChance);
                float cumulative = 0f;
                RareType selectedRarity = RareType.Common;

                foreach (var dropChance in capsuleToySetData.dropChances)
                {
                    cumulative += dropChance.dropChance;
                    if (roll <= cumulative)
                    {
                        selectedRarity = dropChance.rareType;
                        break;
                    }
                }

                // Step 3: Filter toys with the selected rarity
                var matchingToys = capsuleToySetData.capsuleToys
                    .Where(toy => toy.toyRareType == selectedRarity)
                    .ToList();

                if (matchingToys.Count > 0)
                {
                    int randomIndex = Random.Range(0, matchingToys.Count);
                    capsuleToyOpened = matchingToys[randomIndex];

                    Instantiate(capsuleToyOpened.toyPrefab, prize.transform.position, prize.transform.rotation, prize.transform);
                    rareSound = SoundManager.Instance.GetShiningSound(capsuleToySetData.capsuleToys[randomIndex].toyRareType);
                    Debug.Log($"Opened a {selectedRarity} toy: {capsuleToyOpened.toyName}");
                }
                else
                {
                    Debug.LogWarning($"No toys found with rarity: {selectedRarity}");
                }
            }

            StartCoroutine(PlayRareShiningSound(rareSound));

            capsuleStatus = CapsuleStatus.Opened;

            return (capsuleToyOpened, prize);
        }

        IEnumerator PlayRareShiningSound(AudioClip clip)
        {
            yield return new WaitForSeconds(0.3f);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public enum CapsuleStatus
    {
        InsideMachine,
        CarryingByPlayer,
        OpenByPlayer,
        WaitingToOpenOnMenu,
        Opened
    }
}
