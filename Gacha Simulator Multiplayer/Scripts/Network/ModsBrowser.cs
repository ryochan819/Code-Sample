using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ModsBrowser : MonoBehaviour
{
    [SerializeField] private GameObject modListPanel;
    [SerializeField] private Transform modContent;
    [SerializeField] private ModsController modPrefab;
    [SerializeField] private Button nextPage;
    [SerializeField] private Button previousPage;
    [SerializeField] private TMP_InputField searchBar;
    [SerializeField] private Button backgroundButton;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button sortByLatestButton;
    [SerializeField] private Button sortByUpdateButton;
    [SerializeField] private Button sortByVoteScoreButton;
    [SerializeField] private Button closeButton;

    private UGCQueryHandle_t currentQueryHandle;
    private List<SteamUGCDetails_t> mods = new List<SteamUGCDetails_t>();
    private uint currentPage = 1;
    private const int modsPerPage = 50;
    string lastQuery = string.Empty;

    EUGCQuery currentSortType = EUGCQuery.k_EUGCQuery_RankedByVote;

    void Start()
    {
        // Initialize Steamworks
        if (!SteamManager.Initialized) return;

        searchButton.onClick.AddListener(OnSearchClicked);
        nextPage.onClick.AddListener(OnNextPageClicked);
        previousPage.onClick.AddListener(OnPreviousPageClicked);
        sortByVoteScoreButton.onClick.AddListener(() => { currentSortType = EUGCQuery.k_EUGCQuery_RankedByVote; SearchMods(searchBar.text); currentPage = 1;});
        sortByLatestButton.onClick.AddListener(() => { currentSortType = EUGCQuery.k_EUGCQuery_RankedByPublicationDate; SearchMods(searchBar.text); currentPage = 1;});
        sortByUpdateButton.onClick.AddListener(() => { currentSortType = EUGCQuery.k_EUGCQuery_RankedByLastUpdatedDate; SearchMods(searchBar.text); currentPage = 1;});
        clearButton.onClick.AddListener(() => { searchBar.text = string.Empty; SearchMods(string.Empty); });
        closeButton.onClick.AddListener(() => { modListPanel.SetActive(false); });
        backgroundButton.onClick.AddListener(() => { modListPanel.SetActive(false); });

        // Show initial mod list
        SearchMods(searchBar.text);
    }

    void OnDestroy()
    {
        searchButton.onClick.RemoveAllListeners();
        nextPage.onClick.RemoveAllListeners();
        previousPage.onClick.RemoveAllListeners();
        sortByVoteScoreButton.onClick.RemoveAllListeners();
        sortByLatestButton.onClick.RemoveAllListeners();
        sortByUpdateButton.onClick.RemoveAllListeners();
        clearButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }

    private void OnSearchClicked()
    {
        // Perform the search by the text entered in the search bar
        SearchMods(searchBar.text);
    }

    private void SearchMods(string query)
    {
        Debug.Log ("Search mods for app: " + SteamUtils.GetAppID());
        if (query != lastQuery)
        {
            currentPage = 1;
            lastQuery = query;
        }

        // Destroy existing mod UI elements
        foreach (Transform mods in modContent)
        {
            Destroy(mods.gameObject);
        }

        // Use the Steamworks UGC query function to search the workshop by keywords
        SteamUGC.ReleaseQueryUGCRequest(currentQueryHandle);

        currentQueryHandle = SteamUGC.CreateQueryAllUGCRequest(
            currentSortType,
            EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
            SteamUtils.GetAppID(),
            SteamUtils.GetAppID(),
            currentPage
        );

        if (!string.IsNullOrEmpty(query))
        {
            SteamUGC.SetSearchText(currentQueryHandle, query);
        }

        Debug.Log($"Searching for mods with query: {query}");
        // Send the query to Steam
        CallResult<SteamUGCQueryCompleted_t> _ugcQueryCallResult = new CallResult<SteamUGCQueryCompleted_t>();

        SteamAPICall_t querySent = SteamUGC.SendQueryUGCRequest(currentQueryHandle);

        _ugcQueryCallResult.Set(querySent, (result, bIOFailure) =>
        {
            Debug.Log($"Searching for mods _ugcQueryCallResult: {result}");

            if (result.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Failed to query mods.");
                return;
            }

            // Handle no results found (empty page)
            if (result.m_unNumResultsReturned == 0)
            {
                Debug.Log("No mods found for the current page.");
                // Optionally, show a message or update UI elements to indicate no results
                return; // Do not attempt to instantiate anything
            }

            // Populate the mod list with the results
            for (uint i = 0; i < result.m_unNumResultsReturned; i++)
            {
                if (SteamUGC.GetQueryUGCResult(currentQueryHandle, i, out SteamUGCDetails_t modDetails))
                {
                    // Instantiate a new mod UI item (ModsController)
                    ModsController modUI = Instantiate(modPrefab, modContent);
                    uint itemState = SteamUGC.GetItemState(modDetails.m_nPublishedFileId);

                    Debug.Log($"Mod details: {modDetails.m_rgchTitle}, State: {itemState}");

                    // Pass the mod details to the ModsController to populate the UI
                    modUI.Setup(modDetails);

                    // Download and apply the preview image
                    if (modDetails.m_hPreviewFile != UGCHandle_t.Invalid)
                    {
                        CallResult<RemoteStorageDownloadUGCResult_t> _previewDownloadCallResult = new CallResult<RemoteStorageDownloadUGCResult_t>();
                        SteamAPICall_t handle = SteamRemoteStorage.UGCDownload(modDetails.m_hPreviewFile, 0);
                        Debug.Log($"Downloading preview image for mod: {modDetails.m_rgchTitle}");
                        _previewDownloadCallResult.Set(handle, (downloadResult, failure) =>
                        {
                            Debug.Log($"Download preview image call result: {downloadResult}");
                            if (failure || downloadResult.m_eResult != EResult.k_EResultOK)
                            {
                                Debug.LogWarning("Failed to download preview image.");
                                return;
                            }

                            byte[] imageData = new byte[downloadResult.m_nSizeInBytes];
                            int bytesRead = SteamRemoteStorage.UGCRead(downloadResult.m_hFile, imageData, downloadResult.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);

                            if (bytesRead > 0)
                            {
                                Texture2D texture = new Texture2D(2, 2);
                                if (texture.LoadImage(imageData))
                                {
                                    // Now reapply setup with the texture (or create a new method to just set image)
                                    modUI.SetPreviewImage(texture);
                                }
                            }
                        });
                    }
                }
            }

            // Release the query request after processing
            SteamUGC.ReleaseQueryUGCRequest(currentQueryHandle);

            // Optional: Disable or enable pagination buttons depending on the number of results
            if (result.m_unNumResultsReturned < modsPerPage)
            {
                nextPage.interactable = false; // Disable "Next" if no more results
            }
            else
            {
                nextPage.interactable = true;
            }
        });
    }

    private void OnNextPageClicked()
    {
        currentPage++;
        SearchMods(searchBar.text);
    }

    private void OnPreviousPageClicked()
    {
        if (currentPage > 1)
        {
            currentPage--;
            SearchMods(searchBar.text);
        }
    }
}