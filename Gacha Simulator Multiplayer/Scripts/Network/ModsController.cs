using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModsController : MonoBehaviour
{
    [SerializeField] RawImage modImage;
    [SerializeField] TextMeshProUGUI modName;
    [SerializeField] Button subscribeButton;
    bool subscribed = false;

    SteamUGCDetails_t currentModDetails;
    private Steamworks.Callback<DownloadItemResult_t> m_DownloadItemResultCallback;

    void Awake()
    {
        if (SteamManager.Initialized)
        {
            // Create the callback
            m_DownloadItemResultCallback = Steamworks.Callback<DownloadItemResult_t>.Create(DownloadItemResultCallback);
            Debug.Log("DownloadItemResult callback created.");
        }
    }

    public void Setup(SteamUGCDetails_t mod)
    {
        currentModDetails = mod; // Store the current mod details for download result comparison
        uint itemState = SteamUGC.GetItemState(mod.m_nPublishedFileId);
        subscribed = (itemState & (uint)EItemState.k_EItemStateSubscribed) != 0;

        modName.text = mod.m_rgchTitle;
        UpdateButtonUI();

        subscribeButton.onClick.RemoveAllListeners();
        subscribeButton.onClick.AddListener(() =>
        {
            uint updatedItemState = SteamUGC.GetItemState(mod.m_nPublishedFileId);
            bool isCurrentlySubscribed = (updatedItemState & (uint)EItemState.k_EItemStateSubscribed) != 0;

            if (isCurrentlySubscribed)
            {
                SteamUGC.UnsubscribeItem(mod.m_nPublishedFileId);
                Debug.Log("Unsubscribed: " + mod.m_rgchTitle);
            }
            else
            {
                // Subscribe and start downloading
                SteamUGC.SubscribeItem(mod.m_nPublishedFileId);

                bool downloadStarted = SteamUGC.DownloadItem(mod.m_nPublishedFileId, true);
                Debug.Log($"Download Started: {downloadStarted} for mod {mod.m_rgchTitle}");

                if (downloadStarted)
                {
                    Debug.Log("Download started successfully for: " + mod.m_rgchTitle);
                }
                else
                {
                    Debug.LogWarning("Failed to start download for: " + mod.m_rgchTitle);
                }
            }
        });
    }

    void DownloadItemResultCallback(DownloadItemResult_t downloadResult)
    {
        // This debug is to ensure the callback is being triggered
        Debug.Log($"DownloadItemResultCallback: Result={downloadResult.m_eResult}, FileId={downloadResult.m_nPublishedFileId.m_PublishedFileId}");

        if (downloadResult.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"UGC download failed: Result={downloadResult.m_eResult}");
            return;
        }

        // This ensures we're handling the correct mod download result
        if (downloadResult.m_nPublishedFileId.m_PublishedFileId == currentModDetails.m_nPublishedFileId.m_PublishedFileId)
        {
            Debug.Log($"Successfully downloaded mod: {currentModDetails.m_rgchTitle}");
            subscribed = true;
            UpdateButtonUI(); // Update button UI after download
            ModsEvent.ModsListUpdated(subscribed);
        }
        else
        {
            Debug.LogWarning($"Download callback received for a different file: Expected={currentModDetails.m_nPublishedFileId.m_PublishedFileId}, Got={downloadResult.m_nPublishedFileId.m_PublishedFileId}");
        }
    }

    private void UpdateButtonUI()
    {
        // Change subscribe button image
        // subscribeButton.GetComponentInChildren<TextMeshProUGUI>().text = subscribed ? "Unsubscribe" : "Subscribe";
        // test only
        subscribeButton.image.color = subscribed ? Color.green : Color.red;
    }


    public void SetPreviewImage(Texture2D image)
    {
        modImage.texture = image;
    }
}
