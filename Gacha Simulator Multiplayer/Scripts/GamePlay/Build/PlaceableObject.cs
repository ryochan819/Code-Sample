using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public abstract class PlaceableObject : NetworkBehaviour
    {
        private int collisionCount = 0;
        private bool canPlaceOnLayer;
        public bool CanPlaceOnLayer => canPlaceOnLayer;
        private bool lastValidState = true;
        private PlacementState placementState = PlacementState.Preview;

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger)
            {
                collisionCount++;
                UpdatePlacementValidity();

                Debug.Log("Trigger Detected: " + other.name);
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!other.isTrigger)
            {
                collisionCount--;
                UpdatePlacementValidity();
            }
        }

        private void UpdatePlacementValidity()
        {
            bool isCurrentlyValid = collisionCount == 0;

            // Only apply material if the state actually changed
            if (isCurrentlyValid != lastValidState)
            {
                BuildManager.Instance.UpdateMaterial(isCurrentlyValid);
                lastValidState = isCurrentlyValid;
            }
        }

        public virtual void SetCanPlace(bool canPlace)
        {
            if (this.canPlaceOnLayer != canPlace)
            {
                this.canPlaceOnLayer = canPlace;

                bool isCurrentlyValid = GetCanPlace();

                if (isCurrentlyValid != lastValidState)
                {
                    BuildManager.Instance.UpdateMaterial(isCurrentlyValid);
                    lastValidState = isCurrentlyValid;
                }
            }
        }

        public virtual bool GetCanPlace()
        {
            return canPlaceOnLayer && collisionCount == 0;
        }

        public virtual void SetPlacementState(PlacementState state)
        {
            placementState = state;
        }
    }

    public enum PlacementState
    {
        Preview,
        Editing,
        Placed
    }
}
