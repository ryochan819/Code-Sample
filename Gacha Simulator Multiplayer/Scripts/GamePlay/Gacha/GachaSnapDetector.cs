using System.Collections.Generic;
using UnityEngine;

namespace Gacha.gameplay
{
    public class GachaSnapDetector : MonoBehaviour
    {
        [SerializeField] GachaMachinePlaceable gachaMachinePlaceable;
        public List<Collider> detectedColliders = new List<Collider>();

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "GachaMachine" && other.gameObject != transform.parent)
            {
                if (!detectedColliders.Contains(other))
                {
                    detectedColliders.Add(other);
                    gachaMachinePlaceable.UpdateSnapColliders(detectedColliders);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (detectedColliders.Contains(other))
            {
                detectedColliders.Remove(other);
                gachaMachinePlaceable.UpdateSnapColliders(detectedColliders);
            }
        }
    }
}