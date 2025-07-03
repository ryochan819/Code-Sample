using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PurrLobby.Providers;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PurrLobby
{
    public class ModMessager : MonoBehaviour
    {
        [SerializeField] MenuLobbyManager menuLobbyManager;
        [SerializeField] GameObject missingModPage;
        [SerializeField] TextMeshProUGUI missingModText;
        [SerializeField] Button cancelButton;
        [SerializeField] Button downloadButton;
        [SerializeField] GameObject DownloadingModPage;
        [SerializeField] TextMeshProUGUI downloadText;
        [SerializeField] Button confirmButton;

        private bool isMissingModPageActive = false;

        void Awake()
        {
            cancelButton.onClick.AddListener(() =>
            {
                missingModPage.SetActive(false);
            });

            confirmButton.onClick.AddListener(() =>
            {
                DownloadingModPage.SetActive(false);
            });
        }

        public void SetMissingModMessage(List<SteamLobbyProvider.ModInfo> mods, Lobby room)
        {
            if (isMissingModPageActive)
            {
                // prevent multiple pages from opening
                return;
            }

            isMissingModPageActive = true;

            missingModText.text = "Missing Mods:\n";
            foreach (var modID in mods)
            {
                missingModText.text += $"{modID.title}\n";
            }

            missingModPage.SetActive(true);

            List<SteamLobbyProvider.ModInfo> modsToDownload = mods;
            Lobby targetLobby = room;

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                menuLobbyManager.LobbyManager.LeaveLobby();
                missingModPage.SetActive(false);
                isMissingModPageActive = false;
            });

            downloadButton.onClick.RemoveAllListeners();
            downloadButton.onClick.AddListener(async () =>
            {
                missingModPage.SetActive(false);
                downloadButton.gameObject.SetActive(false);
                DownloadingModPage.SetActive(true);

                bool downloadSuccess = await DownloadAllMissingMods(modsToDownload);

                if (downloadSuccess)
                {
                    Debug.Log("All missing mods downloaded successfully.");

                    downloadText.text = "Sync Mods with Host...";

                    await menuLobbyManager.SyncModsWithHostCheck(targetLobby);

                    downloadText.text = "All missing mods downloaded and synced with host.";
                    confirmButton.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("One or more mods failed to download.");
                    downloadText.text = "Some mods failed to download. Please try again.";
                    confirmButton.gameObject.SetActive(true);
                }
            });
        }

        public async UniTask<bool> DownloadAllMissingMods(List<SteamLobbyProvider.ModInfo> modsToDownload)
        {
            List<UniTask<bool>> downloadTasks = new List<UniTask<bool>>();

            foreach (var mod in modsToDownload)
            {
                Debug.Log($"Starting download for: {mod.title} ({mod.id})");

                downloadText.text = $"Downloading {mod.title}..."; // optional: may flash per item

                SteamUGC.SubscribeItem(new PublishedFileId_t(mod.id));

                var modId = new PublishedFileId_t(mod.id);

                bool started = SteamUGC.DownloadItem(modId, true);
                if (!started)
                {
                    Debug.LogError($"Failed to start download for: {mod.title} ({mod.id})");
                    return false;
                }

                // Add the task but don't await yet
                downloadTasks.Add(WaitForModDownload(modId));
            }

            downloadText.text = $"Downloading All Required Mods...";

            // Wait for all download tasks to finish concurrently
            bool[] results = await UniTask.WhenAll(downloadTasks);

            // If any download failed, return false
            if (results.Any(r => r == false))
            {
                Debug.LogError("One or more mods failed to download.");
                return false;
            }

            Debug.Log("All mods downloaded successfully.");
            return true;
        }

        private async UniTask<bool> WaitForModDownload(PublishedFileId_t modId)
        {
            const float checkInterval = 0.5f;
            int stableCount = 0;
            const int maxStableFrames = 20; // 20 x 0.5s = 10s of no download progress

            while (true)
            {
                var state = (EItemState)SteamUGC.GetItemState(modId);

                if (state.HasFlag(EItemState.k_EItemStateInstalled))
                {
                    Debug.Log($"Mod {modId} installed successfully.");
                    return true;
                }

                if (state.HasFlag(EItemState.k_EItemStateDownloading) ||
                    state.HasFlag(EItemState.k_EItemStateDownloadPending))
                {
                    stableCount = 0; // still trying to download, reset fail counter
                }
                else
                {
                    stableCount++;
                }

                if (stableCount >= maxStableFrames)
                {
                    Debug.LogWarning($"Mod {modId} failed to download or stalled.");
                    return false;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(checkInterval));
            }
        }

        void OnDestroy()
        {
            cancelButton.onClick.RemoveAllListeners();
        }
    }
}