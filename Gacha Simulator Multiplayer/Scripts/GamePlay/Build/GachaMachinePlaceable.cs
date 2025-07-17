using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public class GachaMachinePlaceable : PlaceableObject
    {
        List<Collider> snapobjects = new List<Collider>();
        float snapDistance = 0.5f;

        private bool snapTargetsDirty = true;
        private Vector3 lastSnapQueryPosition = Vector3.positiveInfinity;
        private Collider lastSnapTarget = null;
        private const float snapRecheckThreshold = 0.2f;

        [SerializeField] GameObject snapDetector;
        [SerializeField] GameObject placedCollider;
        [SerializeField] Collider coinCollider;
        [SerializeField] Collider handleCollider;
        [SerializeField] NetworkTransform networkTransform;

        public override void SetPlacementState(PlacementState state)
        {
            base.SetPlacementState(state);

            switch (state)
            {
                case PlacementState.Placed:
                    gameObject.layer = LayerMask.NameToLayer("GachaMachine");
                    GetComponent<Collider>().isTrigger = false;
                    snapDetector.SetActive(false);
                    placedCollider.SetActive(true);
                    coinCollider.enabled = true;
                    handleCollider.enabled = true;
                    networkTransform.enabled = false;

                    GiveOwnership(null);
                    break;
                case PlacementState.Editing:
                    gameObject.layer = LayerMask.NameToLayer("Preview");
                    GetComponent<Collider>().isTrigger = true;
                    snapDetector.SetActive(true);
                    placedCollider.SetActive(false);
                    coinCollider.enabled = false;
                    handleCollider.enabled = false;
                    networkTransform.enabled = true;
                    break;
                default:
                    break;
            }
        }

        public void UpdateSnapColliders(List<Collider> colliders)
        {
            snapobjects = colliders;
            snapTargetsDirty = true;
        }

        public Collider GetSnapTarget(Vector3 targetPosition)
        {
            // Avoid rechecking unless needed
            if (!snapTargetsDirty && Vector3.Distance(targetPosition, lastSnapQueryPosition) < snapRecheckThreshold)
            {
                return lastSnapTarget;
            }

            lastSnapQueryPosition = targetPosition;
            snapTargetsDirty = false;

            if (snapobjects.Count == 0)
            {
                lastSnapTarget = null;
                return null;
            }

            Collider closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var target in snapobjects)
            {
                if (target != null)
                {
                    float distance = Vector3.Distance(targetPosition, target.transform.position);
                    if (distance < snapDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = target;
                    }
                }
            }

            lastSnapTarget = closestTarget;
            return closestTarget;
        }
    }
}