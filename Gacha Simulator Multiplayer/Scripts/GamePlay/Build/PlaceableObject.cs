using Gacha.system;
using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public abstract class PlaceableObject : NetworkBehaviour
    {
        private int collisionCount = 0;
        private bool canPlaceOnLayer = true;
        public bool CanPlaceOnLayer => canPlaceOnLayer;
        private bool lastValidState = true;
        private PlacementState placementState = PlacementState.Default;

        MaterialOverrideHandler materialOverrideHandler;

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger)
            {
                collisionCount++;
                UpdatePlacementValidity();

                Debug.Log("Trigger Detected: " + other.name + " collision count: " + collisionCount);
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
                SetMaterial(isCurrentlyValid);
                lastValidState = isCurrentlyValid;
            }
        }

        public virtual void SetCanPlace(bool canPlace)
        {
            if (canPlaceOnLayer != canPlace)
            {
                canPlaceOnLayer = canPlace;

                bool isCurrentlyValid = GetCanPlace();

                if (isCurrentlyValid != lastValidState)
                {
                    SetMaterial(isCurrentlyValid);
                    lastValidState = isCurrentlyValid;
                }
            }
        }

        public virtual bool GetCanPlace()
        {
            return canPlaceOnLayer && collisionCount == 0;
        }
        
        [ObserversRpc(bufferLast: true)]
        public virtual void SetPlacementState(PlacementState state)
        {
            placementState = state;

            switch (state)
            {
                case PlacementState.Placed:
                    RestoreOriginalMaterials();
                    break;
                case PlacementState.Preview:
                case PlacementState.Editing:
                    if (materialOverrideHandler == null)
                    {
                        CacheMaterial();
                    }
                    break;
                default:
                    break;
            }
        }

        [ObserversRpc(bufferLast: true)]
        public void SetMaterial(bool valid)
        {
            if (materialOverrideHandler == null || materialOverrideHandler.materialCacheList == null || materialOverrideHandler.materialCacheList.Count == 0)
            {
                CacheMaterial(); // will set up cache
            }

            materialOverrideHandler.ApplyOverrideMaterial(valid);
            Debug.Log($"SetMaterial called with valid: {valid} for {gameObject.name} in state {placementState}");
        }

        [ObserversRpc(bufferLast: true)]
        public void RestoreOriginalMaterials()
        {
            if (materialOverrideHandler != null)
            {
                materialOverrideHandler.RestoreOriginalMaterials();
            }
        }

        void CacheMaterial()
        {
            materialOverrideHandler = new MaterialOverrideHandler();
            materialOverrideHandler.CacheOriginalMaterials(gameObject);

            // Re-apply material if a state exists
            materialOverrideHandler.ApplyOverrideMaterial(true);
        }
    }

    public enum PlacementState
    {
        Default,
        Preview,
        Editing,
        Placed
    }
}
