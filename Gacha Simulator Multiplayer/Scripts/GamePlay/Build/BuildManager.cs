using Gacha.system;
using Gacha.ui;
using PrimeTween;
using PurrNet;
using UnityEngine;

namespace Gacha.gameplay
{
    public class BuildManager : NetworkBehaviour
    {
        private LayerMask buildableSurfaces;
        public LayerMask BuildableSurfaces => buildableSurfaces;

        private GameObject currentPreview;
        public GameObject CurrentPreview => currentPreview;
        PlaceableObject currentPlaceable;
        public PlaceableObject CurrentPlaceable => currentPlaceable;
        private GameObject currentBuildPrefab;
        BuildScriptable currentBuildScriptable;
        private bool isPlacing;
        public bool IsPlacing => isPlacing;

        private Vector3 lastTweenPosition;
        private const float TWEEN_POSITION_EPSILON = 0.01f;

        Collider currentSnapTarget;

        public void StartBuildMode(BuildScriptable buildScriptable)
        {
            // Update placeable surface by type
            UpdatePlaceableSurface(buildScriptable);
            SetupPreviewObject(buildScriptable);

            GameEventSystem.EnteredBuildModeEvent(true);
            UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.build);
            GameEventSystem.SwitchPlayerState(PlayerState.build);
        }

        public void UpdatePreviewObjectPosition(RaycastHit hit)
        {
            switch (currentBuildScriptable.category)
            {
                case Category.capsuleMachine:
                    HandleCapsuleMachinePosition(hit);
                    break;
                default:
                    Tween.Position(currentPreview.transform, hit.point, 0.1f);
                    break;
            }
        }

        private void HandleCapsuleMachinePosition(RaycastHit hit)
        {
            bool canPlace = !hit.transform.CompareTag("Ground");

            if (currentPlaceable.CanPlaceOnLayer != canPlace)
            {
                currentPlaceable.SetCanPlace(canPlace);
            }

            if (hit.transform.CompareTag("GachaMachine"))
            {
                // Check Can Snap on Top
                GachaMachine gachaMachine = hit.transform.GetComponent<GachaMachine>();

                if (gachaMachine && gachaMachine.CanSnapOnTop())
                {
                    Transform snapTarget = gachaMachine.TopSnapPosition;
                    currentPreview.transform.rotation = snapTarget.rotation;
                    Tween.Position(currentPreview.transform, snapTarget.position, 0.1f);
                }
                else
                {
                    TryTweenPreviewTo(hit.point);
                }
            }
            else if (currentPlaceable.CanPlaceOnLayer && currentPlaceable is GachaMachinePlaceable gachaPlaceable)
            {
                Collider snapTarget = gachaPlaceable.GetSnapTarget(hit.point);

                if (snapTarget != null)
                {
                    currentSnapTarget = snapTarget;
                    SnapToTarget(currentSnapTarget, hit.point);
                }
                else
                {
                    TryTweenPreviewTo(hit.point);
                }
            }
            else
            {
                TryTweenPreviewTo(hit.point);
            }
        }

        private void TryTweenPreviewTo(Vector3 targetPosition)
        {
            if (Vector3.Distance(lastTweenPosition, targetPosition) > TWEEN_POSITION_EPSILON)
            {
                lastTweenPosition = targetPosition;
                Tween.Position(currentPreview.transform, targetPosition, 0.1f);
            }
        }

        private void SnapToTarget(Collider target, Vector3 hitPoint)
        {
            Vector3 toHitPoint = hitPoint - target.transform.position;
            float side = Vector3.Dot(target.transform.right, toHitPoint.normalized);

            float floorY = 0.137f;
            Vector3 snapOffset = (side >= 0)
                ? target.transform.right * 0.31f  // Right side
                : target.transform.right * -0.31f; // Left side

            Vector3 snapPosition = target.transform.position + snapOffset;
            snapPosition.y = floorY;

            currentPreview.transform.position = snapPosition;
            currentPreview.transform.rotation = target.transform.rotation;
        }

        private void UpdatePlaceableSurface(BuildScriptable buildScriptable)
        {
            switch (buildScriptable.category)
            {
                case Category.capsuleMachine:
                    buildableSurfaces = LayerMask.GetMask("Floor", "GachaMachine");
                    break;
                default:
                    buildableSurfaces = LayerMask.GetMask("Floor", "Default");
                    break;
            }
        }

        private void SetupPreviewObject(BuildScriptable buildScriptable)
        {
            currentBuildScriptable = buildScriptable;
            currentBuildPrefab = buildScriptable.prefab;
            
            Transform playerTransform = GameSceneDataManager.instance.LocalPlayer.transform; // or Camera.main.transform, if that's better
            Vector3 spawnPosition = playerTransform.position + playerTransform.forward * 2f; // 2 units in front
            Quaternion spawnRotation = Quaternion.identity;
            currentPreview = Instantiate(currentBuildPrefab, spawnPosition, spawnRotation);
            
            currentPlaceable = currentPreview.GetComponent<PlaceableObject>();
            currentPlaceable.SetPlacementState(PlacementState.Preview);
            isPlacing = true;
        }

        public void LeaveBuildMode()
        {
            isPlacing = false;
            currentPreview = null;
            currentBuildPrefab = null;
            currentBuildScriptable = null;

            GameEventSystem.EnteredBuildModeEvent(false);
            UIEventSystem.ChangeUIInterfaceState(GameSceneInterfaceManager.GameStates.idle);
            GameEventSystem.SwitchPlayerState(PlayerState.idle);
        }

        public void ConfirmPlacement()
        {
            if (CanPlaceHere())
            {
                Debug.Log("Can place confirmed");
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

        public void AdjustPlacementRotation(float rotation)
        {
            currentPreview.transform.Rotate(Vector3.up, rotation);
        }
    }
}
