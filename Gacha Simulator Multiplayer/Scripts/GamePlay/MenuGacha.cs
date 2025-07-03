using System;
using System.Collections;
using System.Collections.Generic;
using Gacha.gameplay;
using Gacha.system;
using PrimeTween;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;

public class MenuGacha : MonoBehaviour
{
    private GameObject selectedObject;
    private bool isDragging = false;
    bool isEnoughCoins = false;
    private Vector3 centerPoint;
    private Vector3 lastMousePosition;
    private float limitedRotation = 0f;
    private bool reachedLimit = false;
    private GameObject rayCastObject;
    (CapsuleToyEntry, GameObject) currentCapsuleController = (null, null);

    [SerializeField] GachaMachine gachaMachine;
    [SerializeField] Transform capsuleTarget;
    [SerializeField] Collider handleCollider;
    [SerializeField] Collider coinInsertCollider;
    [SerializeField] GameObject uiBlocker;
    [SerializeField] TextMeshProUGUI uiBlockerRarityText;
    [SerializeField] TextMeshProUGUI uiBlockerCapsuleText;
    [SerializeField] GameObject menuGachaRecordUIParent;
    [SerializeField] GameObject menuGachaRecordPrefab;

    void Start()
    {
        CapsuleToySetData capsuleToySetData = GameReference.Instance.LocalCapsuleToySets[UnityEngine.Random.Range(0, GameReference.Instance.LocalCapsuleToySets.Length)];
        gachaMachine.SetCapsuleMachineToySet(capsuleToySetData);
    }

    void Update()
    {
        HighlightInteractableObjects();
        HandleMouseDown();
        HandleMouseDrag();
        HandleMouseUp();
    }

    private void HighlightInteractableObjects()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Interactable", "Capsule");

        // debug mouse to screen position
        // Debug.Log("Mouse Position: " + Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != rayCastObject)
            {
                // Disable previous outline
                if (rayCastObject != null)
                {
                    Outline outline = rayCastObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }

                // Enable new outline
                if (hitObject.CompareTag("GachaSwitch") || hitObject.CompareTag("CoinInsert") || hitObject.CompareTag("Capsule"))
                {
                    Outline outline = hitObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = true;
                    }
                    rayCastObject = hitObject;
                }
                else
                {
                    rayCastObject = null;
                }
            }
        }
        else
        {
            if (rayCastObject != null)
            {
                rayCastObject.GetComponent<Outline>().enabled = false;
                rayCastObject = null;
            }
        }
    }

    private void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Interactable", "Capsule");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("ray cast hit: " + hit.transform.name);
            if (hit.collider.CompareTag("GachaSwitch"))
            {
                selectedObject = hit.collider.gameObject;
                centerPoint = Camera.main.WorldToScreenPoint(selectedObject.transform.position);
                lastMousePosition = Input.mousePosition;
                isDragging = true;
                isEnoughCoins = selectedObject.GetComponent<GachaHandle>().GetIsEnoughCoins();
                Debug.Log("Enough Coins: " + isEnoughCoins);
            }

            if (hit.collider.CompareTag("CoinInsert"))
            {
                bool coinAdded = hit.collider.GetComponent<GachaCoinInsert>().AddCoins();

                if (coinAdded)
                {
                    // Do something
                }
            }

            if (hit.collider.CompareTag("Capsule"))
            {
                CapsuleController capsuleController = hit.transform.GetComponent<CapsuleController>();

                if (capsuleController.CapsuleStatus == CapsuleStatus.InsideMachine)
                {
                    capsuleController.SetCapsuleStatus(CapsuleStatus.WaitingToOpenOnMenu);
                    StartCoroutine(Utility.BounceToPosition(hit.transform, capsuleTarget.position, capsuleTarget.rotation, 0.5f, 0.1f));
                    handleCollider.enabled = false;
                    coinInsertCollider.enabled = false;
                    Debug.Log("Capsule clicked: " + hit.collider.name);
                    return;
                }

                if (capsuleController.CapsuleStatus == CapsuleStatus.WaitingToOpenOnMenu)
                {
                    currentCapsuleController = capsuleController.OpenCapsule();
                    uiBlockerCapsuleText.text = currentCapsuleController.Item1.toyName;
                    string rareText = currentCapsuleController.Item1.toyRareType.ToString();
                    uiBlockerRarityText.text = "<link=" + rareText + ">" + rareText + "</link>";
                    uiBlocker.SetActive(true);
                }
            }
        }
        else
        {
            selectedObject = null;
        }

    }

    private void HandleMouseDrag()
    {
        if (!Input.GetMouseButton(0) || !isDragging || selectedObject == null || selectedObject.tag != "GachaSwitch")
            return;

        Vector2 currMousePosition = Input.mousePosition;

        GachaHandle gachaHandle = selectedObject.GetComponent<GachaHandle>();

        Debug.Log("Dragging: " + selectedObject.name + ", Enough Coins: " + isEnoughCoins);

        if (isEnoughCoins)
        {
            Vector2 prevDir = lastMousePosition - centerPoint;
            Vector2 currDir = currMousePosition - (Vector2)centerPoint;

            float angleDifference = prevDir.SignedAngleTo(currDir);

            if (angleDifference < 0) // Only rotate clockwise
            {
                selectedObject.transform.Rotate(0, 0, -angleDifference);
                bool rotateComplete = gachaHandle.HandleRotated();
                if (rotateComplete)
                {
                    isDragging = false;
                    gachaHandle.ResetHandle(false);
                    return;
                }
            }

            lastMousePosition = currMousePosition;
        }
        else if (!reachedLimit)
        {
            Vector2 prevDir = lastMousePosition - centerPoint;
            Vector2 currDir = currMousePosition - (Vector2)centerPoint;

            float angleDifference = prevDir.SignedAngleTo(currDir);

            if (angleDifference < 0) // Only clockwise
            {
                float remaining = 5f - limitedRotation;
                float rotationToApply = Mathf.Max(angleDifference, -remaining); // angleDifference is negative

                selectedObject.transform.Rotate(0, 0, -rotationToApply);
                limitedRotation += -rotationToApply; // make positive

                if (limitedRotation >= 5f)
                {
                    reachedLimit = true;
                    gachaHandle.ResetHandle();
                }
            }

            lastMousePosition = currMousePosition;
        }
    }

    private void HandleMouseUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            lastMousePosition = Input.mousePosition; // Reset to prevent jumps
            isDragging = false;
            selectedObject = null;
            isEnoughCoins = false;
            reachedLimit = false;
            limitedRotation = 0f;
        }
    }

    // UI Blocker Function
    private Dictionary<string, (CapsuleToyEntry entry, GameObject uiRecord, int count)> drawnToys = new();
    [SerializeField] Camera uiCamera;
    public void UIBlockerClicked()
    {
        uiBlocker.SetActive(false);

        if (currentCapsuleController != (null, null))
        {
            CapsuleToyEntry toy = currentCapsuleController.Item1;
            GameObject capsuleObject = currentCapsuleController.Item2.transform.parent.gameObject; // item2

            string toyKey = toy.toyName;
            GameObject uiRecord;

            if (!drawnToys.ContainsKey(toyKey))
            {
                GameObject newRecord = Instantiate(menuGachaRecordPrefab, menuGachaRecordUIParent.transform);
                newRecord.GetComponent<Image>().sprite = toy.toyImage;
                newRecord.GetComponentInChildren<TextMeshProUGUI>().text = "1";

                drawnToys[toyKey] = (toy, newRecord, 1);
                uiRecord = newRecord;
            }
            else
            {
                var (entry, uiObj, count) = drawnToys[toyKey];
                count++;
                uiObj.GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();
                drawnToys[toyKey] = (entry, uiObj, count);
                uiRecord = uiObj;
            }

            Tween.Scale(capsuleObject.transform, Vector3.zero, 0.3f).OnComplete(() =>
            {
                Destroy(capsuleObject);
                currentCapsuleController = (null, null);
            //     Vector3 worldPosition = uiCamera.ScreenToWorldPoint(uiRecord.transform.position);
            //     Tween.Position(capsuleObject.transform, worldPosition, 0.5f).OnComplete(() =>
            // {
            //     Destroy(capsuleObject);
            //     currentCapsuleController = (null, null);
            // });
            });
           
        }

        handleCollider.enabled = true;
        coinInsertCollider.enabled = true;
    }
}

