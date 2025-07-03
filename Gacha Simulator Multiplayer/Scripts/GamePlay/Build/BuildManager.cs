using System;
using System.Collections.Generic;
using Gacha.system;
using Gacha.ui;
using UnityEngine;

namespace Gacha.gameplay
{
    public class BuildManager : MonoBehaviour
    {
        public static BuildManager Instance { get; private set; }

        [SerializeField] private LayerMask buildableSurfaces;
        public LayerMask BuildableSurfaces => buildableSurfaces;

        private GameObject currentPreview;
        public GameObject CurrentPreview => currentPreview;
        PlaceableObject currentPlaceable;
        public PlaceableObject CurrentPlaceable => currentPlaceable;
        private GameObject currentBuildPrefab;
        private bool isPlacing;
        public bool IsPlacing => isPlacing;

        private MaterialOverrideHandler materialOverrideHandler = new MaterialOverrideHandler();

        private void Awake()
        {
            Instance = this;
        }

        public void StartBuildMode(BuildScriptable buildScriptable)
        {
            // Update placeable surface by type
            UpdatePlaceableSurface(buildScriptable);
            SetupPreviewObject(buildScriptable);

            GameEventSystem.EnteredBuildModeEvent(true);
            UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.build);
            GameEventSystem.SwitchPlayerState(PlayerState.build);
        }

        private void UpdatePlaceableSurface(BuildScriptable buildScriptable)
        {
            switch (buildScriptable.category)
            {
                case Category.capsuleMachine:
                    buildableSurfaces = LayerMask.GetMask("Floor");
                    break;
                default:
                    buildableSurfaces = LayerMask.GetMask("Floor", "Default");
                    break;
            }
        }

        private void SetupPreviewObject(BuildScriptable buildScriptable)
        {
            currentBuildPrefab = buildScriptable.prefab;
            currentPreview = Instantiate(currentBuildPrefab);
            currentPlaceable = currentPreview.GetComponent<PlaceableObject>();
            materialOverrideHandler.CacheOriginalMaterials(currentPreview);
            materialOverrideHandler.ApplyOverrideMaterial(GameReference.Instance.ValidPlacementMaterial);
            isPlacing = true;
        }

        public void UpdateMaterial(bool valid)
        {
            Material updateMaterial = valid ? GameReference.Instance.ValidPlacementMaterial : GameReference.Instance.InvalidPlacementMaterial;
            materialOverrideHandler.ApplyOverrideMaterial(updateMaterial);
        }

        public void LeaveBuildMode()
        {
            isPlacing = false;
            if (currentPreview) Destroy(currentPreview);
            currentBuildPrefab = null;

            materialOverrideHandler.ResetHandler();

            GameEventSystem.EnteredBuildModeEvent(false);
            UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.idle);
            GameEventSystem.SwitchPlayerState(PlayerState.idle);
        }

        public void ConfirmPlacement()
        {
            if (CanPlaceHere())
            {
                // Instantiate(currentBuildPrefab, currentPreview.transform.position, currentPreview.transform.rotation);

                // Use currentPreview instead of instantiate new one
                currentPlaceable.SetPlacementState(PlacementState.Placed);
                LeaveBuildMode();
            }
        }

        public Vector3 SnapToGrid(Vector3 point)
        {
            float gridSize = 1f;
            return new Vector3(
                Mathf.Round(point.x / gridSize) * gridSize,
                Mathf.Round(point.y / gridSize) * gridSize,
                Mathf.Round(point.z / gridSize) * gridSize
            );
        }

        private bool CanPlaceHere()
        {
            return currentPlaceable.GetCanPlace();
        }
    }
}
