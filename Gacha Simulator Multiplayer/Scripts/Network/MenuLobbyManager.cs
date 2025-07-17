using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Gacha.system;
using PurrLobby.Providers;
using PurrNet;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrLobby
{
    public class MenuLobbyManager : MonoBehaviour
    {
        [SerializeField] LobbyManager lobbyManager;
        public LobbyManager LobbyManager => lobbyManager;
        [SerializeField] SteamLobbyProvider steamLobbyProvider;
        [SerializeField] LobbyPlayer[] lobbyPlayers;
        [SerializeField] NetworkPrefabs localNetworkPrefabs;
        [SerializeField] NetworkPrefabs gameNetworkPrefabsWithMods;
        [SerializeField] TextMeshProUGUI lobbyText;
        [SerializeField] ModMessager modMessager;
        [PurrScene, SerializeField] string nextScene;

        [HideInInspector]
        public bool activiateInviteOverlayWhenCreateRoom = false;
        [HideInInspector]
        public bool localModsReady;
        List<SteamLobbyProvider.LocalModInfo> subscribedMods = new List<SteamLobbyProvider.LocalModInfo>();

        void Awake()
        {
            gameNetworkPrefabsWithMods.prefabs = localNetworkPrefabs.prefabs
            .Select(p =>
            {
                // Debug.Log($"[PrefabSetup] Prefab: {p.prefab.name}, Pooled: {p.pooled}, WarmupCount: {p.warmupCount}");

                return new NetworkPrefabs.UserPrefabData
                {
                    prefab = p.prefab,
                    pooled = p.pooled,
                    warmupCount = p.warmupCount
                };
            })
            .ToList();

            SubscribeLobbyEvents();
        }

        void Start()
        {
            bool shouldRunJoinCheck = GameManager.Instance.initialLaunchCheck;
            GameManager.Instance.initialLaunchCheck = false;

            bool joinedFromLaunch = shouldRunJoinCheck && CheckLobbyInviteLaunch();
            Debug.Log("Is joining lobby: " + joinedFromLaunch);

            if (!joinedFromLaunch)
            {
                SetupLocalPlayerAvatar();
                HandleLocalModsTask();
            }
        }

        #region Handle Local Data
        private void SetupLocalPlayerAvatar()
        {
            foreach (var lobbyPlayer in lobbyPlayers)
            {
                lobbyPlayer.gameObject.SetActive(false);
            }

            lobbyText.gameObject.SetActive(false);

            if (!SteamManager.Initialized)
            {
                return;
            }

            Debug.Log("Setup local player avatar");
            lobbyText.text = "Lobby (1/4)";
            lobbyText.gameObject.SetActive(true);

            CSteamID localId = SteamUser.GetSteamID();
            int avatarInt = SteamFriends.GetLargeFriendAvatar(localId);

            if (avatarInt == -1)
            {
                Debug.LogWarning("Steam avatar not ready yet.");
                return;
            }

            if (SteamUtils.GetImageSize(avatarInt, out uint width, out uint height))
            {
                byte[] imageBuffer = new byte[width * height * 4];

                if (SteamUtils.GetImageRGBA(avatarInt, imageBuffer, imageBuffer.Length))
                {
                    Texture2D avatarImage = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    avatarImage.LoadRawTextureData(imageBuffer);
                    avatarImage.Apply();

                    // Optional: Flip vertically
                    avatarImage = FlipTextureVertically(avatarImage);

                    lobbyPlayers[0].SetupLocalPlayer(avatarImage);
                    lobbyPlayers[0].gameObject.SetActive(true);
                    lobbyPlayers[1].SetInvite("-1");
                    lobbyPlayers[1].gameObject.SetActive(true);
                }
            }
        }

        private Texture2D FlipTextureVertically(Texture2D original)
        {
            Color[] pixels = original.GetPixels();
            int width = original.width;
            int height = original.height;
            Color[] flipped = new Color[pixels.Length];

            for (int y = 0; y < height; y++)
            {
                Array.Copy(pixels, y * width, flipped, (height - y - 1) * width, width);
            }

            original.SetPixels(flipped);
            original.Apply();
            return original;
        }

        public void HandleLocalModsTask()
        {
            RunTask(async () =>
            {
                await HandleLocalMods();
            });
        }

        async Task HandleLocalMods()
        {
            localModsReady = false;
            Debug.Log("Getting local mods...");
            subscribedMods = await steamLobbyProvider.GetLocalPlayerModsWithDirectoriesAsync();
            Debug.Log($"Subscribed mods: {subscribedMods.Count}");

            foreach (var mod in subscribedMods)
            {
                string modDirectory = mod.InstallDirectory;
                Debug.Log($"Processing mod: {mod.Title} at {modDirectory}");

                // Log all files in the directory
                string[] allFiles = Directory.GetFiles(modDirectory, "*.json", SearchOption.AllDirectories);
                string[] bundlePath = Directory.GetFiles(modDirectory, "*.bundle", SearchOption.AllDirectories);

                if (allFiles.Length == 0)
                {
                    Debug.Log($"No JSON files found in directory {modDirectory}");
                    continue;
                }

                Debug.Log($"Found {bundlePath.Length} bundle files in {modDirectory}");
                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath[0]);
                Debug.Log($"Bundle loaded successfully: {Path.GetFileName(bundlePath[0])}" + " Bundle name: " + bundle.name);
                // Load all assets in the bundle
                string[] bundleFiles = bundle.GetAllAssetNames();
                foreach (var assetName in bundleFiles)
                {
                    Debug.Log("AssetName in bundle: " + assetName);
                }

                foreach (var file in allFiles)
                {
                    Debug.Log($"Found JSON file: {file}");

                    // Read the JSON content
                    string jsonContent = File.ReadAllText(file);

                    // Now parse the JSON into an object, for example, GameObjectData
                    ModJsonData modData = JsonUtility.FromJson<ModJsonData>(jsonContent);

                    if (modData != null)
                    {
                        string fullAssetPath = bundleFiles.FirstOrDefault(path =>
                        Path.GetFileNameWithoutExtension(path) == modData.gameObjects[0].objectName &&
                        path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase));

                        Debug.Log($"Loaded mod data: {modData.gameObjects.Length} game objects found." + " Mod name: " + modData.gameObjects[0].objectName);
                        GameObject prefab = bundle.LoadAsset<GameObject>(fullAssetPath);
                        Debug.Log($"Prefab loaded: {fullAssetPath}");

                        ModScriptable modScriptable = ScriptableObject.CreateInstance<ModScriptable>();
                        modScriptable.prefab = prefab;
                        modScriptable.modName = mod.Title;

                        GameReference.Instance.modScriptables.Add(modScriptable);
                        GameReference.Instance.GameObjects.Add(prefab);
                        // Instantiate(prefab);
                    }
                    else
                    {
                        Debug.LogError("Failed to parse the JSON data.");
                    }
                }

                bundle.Unload(false);
            }
        }

        private List<SteamLobbyProvider.ModInfo> lastHostModId = new List<SteamLobbyProvider.ModInfo>();
        private List<ulong> lastMissingModIds = null;
        private bool hasSyncedModsWithHost = false;

        public async Task<bool> SyncModsWithHostCheck(Lobby room)
        {
            var lobbyID = new CSteamID(ulong.Parse(room.LobbyId));

            Debug.Log($"Syncing mods with host, lobby ID: {lobbyID}");

            // Step 1: Get host's required mods
            List<SteamLobbyProvider.ModInfo> hostsModId = await steamLobbyProvider.GetRequiredModInfosFromLobbyAsync(lobbyID);
            if (hostsModId == null || hostsModId.Count == 0)
            {
                Debug.Log("No required mods from host.");
                hasSyncedModsWithHost = true;
                return true;
            }

            Debug.Log($"Host's required mods: {string.Join(", ", hostsModId.Select(m => m.id))}");

            // Step 2: Refresh local subscribed mods
            subscribedMods = await steamLobbyProvider.GetLocalPlayerModsWithDirectoriesAsync();
            HashSet<ulong> localModIds = new HashSet<ulong>(subscribedMods.Select(mod => mod.PublishedFileId));

            // Step 3: Check for missing mods
            List<SteamLobbyProvider.ModInfo> missingMods = hostsModId
                .Where(mod => !localModIds.Contains(mod.id))
                .ToList();

            if (missingMods.Count > 0)
            {
                Debug.Log($"Missing required mods: {string.Join(", ", missingMods.Select(m => m.id))}");

                if (lobbyManager != null)
                {
                    lobbyManager.SetIsReady(SteamUser.GetSteamID().m_SteamID.ToString(), false);
                }

                if (modMessager != null)
                {
                    modMessager.SetMissingModMessage(missingMods, room);
                    Debug.Log("modMessager is not NULL!");
                }
                else
                {
                    Debug.LogError("modMessager is NULL!");
                }

                lastHostModId = hostsModId;
                lastMissingModIds = missingMods.Select(m => m.id).ToList();
                hasSyncedModsWithHost = false;
                return false;
            }

            // Step 4: Determine if sync is needed
            bool previouslyMissingMods = lastMissingModIds != null && lastMissingModIds.Count > 0;
            bool hostModListChanged = !hostsModId.Select(m => m.id).OrderBy(id => id)
                .SequenceEqual(lastHostModId.Select(m => m.id).OrderBy(id => id));

            if ((hostModListChanged || previouslyMissingMods) && !hasSyncedModsWithHost)
            {
                Debug.Log("Syncing mods with host...");

                ModsEvent.SyncingWithHost(true);
                lobbyManager.SetIsReady(SteamUser.GetSteamID().m_SteamID.ToString(), false);

                try
                {
                    await SyncModsWithHost(hostsModId, room);
                }
                catch (Exception ex)
                {
                    Debug.Log($"SyncModsWithHost threw: {ex}");
                }

                lastHostModId = hostsModId;
                lastMissingModIds = null;
                hasSyncedModsWithHost = true;
            }

            Debug.Log("No missing mods, syncing complete.");
            return true;
        }

        private async UniTask SyncModsWithHost(List<SteamLobbyProvider.ModInfo> hostsModId, Lobby room)
        {
            Debug.Log("Syncing mods with host unitask...");

            await UniTask.Yield();

            // Handle mods with hostsModId

            // When mod sync is complete, set the local player to ready
            lobbyManager.SetIsReady(Steamworks.SteamUser.GetSteamID().m_SteamID.ToString(), true);
            ModsEvent.SyncingWithHost(false);

            Debug.Log("Mod sync complete, local player set to ready.");
        }
        #endregion

        #region Lobby Functions
        private bool CheckLobbyInviteLaunch()
        {
            if (!SteamManager.Initialized)
            {
                return false;
            }

            string commandLine;
            bool gotArgs = SteamApps.GetLaunchCommandLine(out commandLine, 10240) > 0;

            var unityArgs = System.Environment.GetCommandLineArgs();

            if (unityArgs.Length >= 2)
            {
                // loop to the 2nd last one, because we are gonna do a + 1
                // the lobbyID is straight after +connect_lobby
                for (int i = 0; i < unityArgs.Length - 1; i++)
                {
                    if (unityArgs[i].ToLower() == "+connect_lobby")
                    {
                        if (ulong.TryParse(unityArgs[i + 1], out ulong lobbyID))
                        {
                            if (lobbyID > 0)
                            {
                                // do something with your lobby id
                                lobbyManager.JoinLobby(lobbyID.ToString());
                                Debug.Log("Joining lobby via launch args: " + lobbyID);
                                return true;
                            }
                        }
                        break;
                    }
                }
            }

            if (!gotArgs)
            {
                // Debug.Log("No Steam launch command line detected.");
                return false;
            }

            // Debug.Log("Launch Command Line: " + commandLine);

            string[] args = commandLine.Split(' ');

            for (int i = 0; i < args.Length; i++)
            {
                // Debug.Log("Arg: " + args[i]);

                // Handle "+connect_lobby <id>"
                if (args[i] == "+connect_lobby" && i + 1 < args.Length)
                {
                    if (ulong.TryParse(args[i + 1], out ulong lobbyId))
                    {
                        CSteamID steamLobbyId = new CSteamID(lobbyId);
                        Debug.Log("Joining lobby via launch args: " + steamLobbyId);
                        lobbyManager.JoinLobby(steamLobbyId.ToString());
                        return true;
                    }
                }

                // Optional: detect steam://joinlobby links
                if (args[i].StartsWith("steam://joinlobby/"))
                {
                    Debug.Log("Detected joinlobby URL: " + args[i]);
                    return true;
                }
            }

            Debug.Log("No valid lobby ID found in launch arguments.");

            return false;
        }

        private void HandleExistingMembers(Lobby room)
        {
            // List<LobbyUser> lobbyUsers = await lobbyManager.CurrentProvider.GetLobbyMembersAsync();
            // foreach (var lobbyUser in lobbyUsers)
            // {
            //     Debug.Log($"Lobby user: {lobbyUser.Id}, IsReady: {lobbyUser.IsReady}");
            // }

            var localUserId = SteamUser.GetSteamID().ToString();

            if (string.IsNullOrEmpty(localUserId))
            {
                Debug.Log($"Can't toggle ready state, local user ID is null or empty.");
                return;
            }
            // Debug.Log($"Local user ID: {localUserId}" + " Room member count: " + room.Members.Count + " room id: " + room.LobbyId);

            int maxPlayers = lobbyManager.createRoomArgs.maxPlayers;
            int remotePlayerIndex = 1; // Start from 1 because 0 is reserved for local player

            // Setup existing players
            for (int i = 0; i < room.Members.Count; i++)
            {
                // Debug.Log("Lobby manager loddy holder: " + lobbyManager.CurrentLobby.IsOwner + " room owner id: " + lobbyManager.CurrentLobby.OwnerId);
                // Debug.Log("Room member id: " + room.Members[i].Id + " isOwner: " + room.IsOwner + " owner id: " + room.OwnerId);
                if (localUserId == room.Members[i].Id)
                {
                    lobbyPlayers[0].SetupPlayer(room.Members[i], room.IsOwner, room.Members[i].IsReady, true);
                    lobbyPlayers[0].gameObject.SetActive(true);
                }
                else
                {
                    if (remotePlayerIndex < lobbyPlayers.Length)
                    {
                        lobbyPlayers[remotePlayerIndex].SetupPlayer(room.Members[i], room.OwnerId == room.Members[i].Id, room.Members[i].IsReady, false);
                        lobbyPlayers[remotePlayerIndex].gameObject.SetActive(true);
                        remotePlayerIndex++;
                    }
                    else
                    {
                        Debug.Log($"Not enough lobby players available to setup player {room.Members[i].Id}.");
                    }
                }
            }

            // Add invite slot (if there's still room)
            int inviteIndex = room.Members.Count;
            if (inviteIndex < maxPlayers && inviteIndex < lobbyPlayers.Length)
            {
                lobbyPlayers[inviteIndex].SetInvite(room.LobbyId);
                lobbyPlayers[inviteIndex].gameObject.SetActive(true);
                inviteIndex++;
            }

            // Disable remaining slots
            for (int i = inviteIndex; i < lobbyPlayers.Length; i++)
            {
                lobbyPlayers[i].gameObject.SetActive(false);
            }
        }

        private void OnModListUpdated(bool subscribed)
        {
            if (lobbyManager.CurrentLobby.IsValid)
            {
                if (subscribed)
                {
                    if (lobbyManager.CurrentLobby.IsOwner)
                    {
                        _ = HandleLocalModsAsyncUpdateLobby(lobbyManager.CurrentLobby);
                    }
                }
                else
                {
                    if (lobbyManager.CurrentLobby.IsOwner)
                    {
                        _ = HandleLocalModsAsyncUpdateLobby(lobbyManager.CurrentLobby);
                    }
                    else
                    {
                        _ = SyncModsWithHostCheck(lobbyManager.CurrentLobby);
                    }
                }
            }
            else
            {
                HandleLocalModsTask();
            }
        }

        async Task HandleLocalModsAsyncUpdateLobby(Lobby room)
        {
            await HandleLocalMods();
            CSteamID lobbyId = new CSteamID(ulong.Parse(room.LobbyId));
            List<ulong> newRequiredMods = steamLobbyProvider.GetRequiredModsFromLobby(lobbyId);
            steamLobbyProvider.SetRequiredModsToLobby(lobbyId, newRequiredMods);
        }

        int _taskLock = 0;

        public async Task WaitForAllTasksAsync()
        {
            while (_taskLock > 0)
            {
                await Task.Yield();
            }
        }

        private async void RunTask(Func<Task> taskFunc)
        {
            if (taskFunc == null) return;

            _taskLock++;
            try
            {
                await taskFunc();
            }
            catch (Exception ex)
            {
                Debug.Log("Task failed:" + ex);
            }
            finally
            {
                _taskLock--;
                if (_taskLock < 0)
                    _taskLock = 0;
            }
        }
        #endregion

        #region Handle Lobby Events
        private void OnRoomCreated(Lobby room)
        {
            Debug.Log("Room created, overlay:" + activiateInviteOverlayWhenCreateRoom);
            if (activiateInviteOverlayWhenCreateRoom)
            {
                // Use steaminviteOverlay to invite player to the lobby
                Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(ulong.Parse(room.LobbyId)));
                Debug.Log("Activating invite overlay for lobby: " + room.LobbyId);
                activiateInviteOverlayWhenCreateRoom = false;
            }
        }

        private bool isSyncingWithHost = false;
        bool IsStarting = false;

        private async Task OnRoomUpdated(Lobby room)
        {
            lobbyText.text = "Lobby (" + room.Members.Count + "/4)";

            HandleExistingMembers(room);
            bool modSynced = false;

            if (!room.IsOwner && !isSyncingWithHost)
            {
                isSyncingWithHost = true;
                try
                {
                    modSynced = await SyncModsWithHostCheck(room);
                }
                finally
                {
                    isSyncingWithHost = false;
                }
            }

            Debug.Log("Room Updated: room owner: " + room.IsOwner + " host id: " + room.OwnerId);

            if (modSynced && !room.IsOwner)
            {
                // Check if host is ready
                bool isHostReady = await lobbyManager.IsHostReadyAsync();

                Debug.Log("Host ready: " + isHostReady);

                if (isHostReady && !IsStarting)
                {
                    //Prevent calling ready again if lobby is updated after all ready
                    IsStarting = true;
                    await LoadScene(false);
                }
            }
        }

        public async Task LoadScene(bool isHost)
        {
            Debug.Log("Loading scene: " + nextScene);
            if (isHost)
            {
                await lobbyManager.SetLobbyStarted();
            }
            await SceneManager.LoadSceneAsync(nextScene);
        }

        private void OnLobbyJoinRequested()
        {
            Debug.Log("Join requested");
        }

        public async Task OnRoomJoined(Lobby room)
        {
            Debug.Log("Room joined" + " room id: " + room.LobbyId + " room owner id: " + room.OwnerId + " isOwner: " + room.IsOwner);

            // sync with host
            if (!room.IsOwner)
            {

                lobbyText.text = "Lobby (" + room.Members.Count + "/4)";
                lobbyText.gameObject.SetActive(true);

                // sync with host's mod
                Debug.Log("Syncing with host's mod...");
                var localUserId = SteamUser.GetSteamID().ToString();
                bool isSyncedWithHost = await SyncModsWithHostCheck(room);
                if (!isSyncedWithHost)
                {
                    Debug.Log("Not synced with host, waiting for mods to sync...");
                    foreach (var member in room.Members)
                    {
                        if (member.Id == localUserId)
                        {
                            lobbyPlayers[0].SetupPlayer(member, room.IsOwner, false, true);
                        }
                    }
                }
                else
                {
                    Debug.Log("Synced with host, setting local player to ready...");
                    lobbyManager.SetIsReady(localUserId, true);
                }
            }

            Debug.Log("Room joined finish");
        }

        private void OnRoomJoinFailed(string arg0)
        {
            Debug.Log("Room join failed: " + arg0);
        }

        public void OnRoomLeft()
        {
            Debug.Log("Room left");
            SetupLocalPlayerAvatar();
        }

        public void OnBrowseClicked()
        {
            Debug.Log("Browse clicked");
        }

        public void OnRoomCreateClicked()
        {
            Debug.Log("Room create clicked");
        }

        public void OnJoiningRoom()
        {
            Debug.Log("Joining room");
        }

        public void OnLeaveBrowseClicked()
        {
            Debug.Log("Leave browse clicked");
        }
        #endregion

        void OnDestroy()
        {
            UnsubscribeLobbyEvents();
        }

        private void SubscribeLobbyEvents()
        {
            lobbyManager.OnRoomCreated.AddListener(OnRoomCreated);
            lobbyManager.OnJoinRequested.AddListener(OnLobbyJoinRequested);
            lobbyManager.OnRoomJoined.AddListener(async (Lobby room) => await OnRoomJoined(room));
            lobbyManager.OnRoomJoinFailed.AddListener(OnRoomJoinFailed);
            lobbyManager.OnRoomUpdated.AddListener(async (Lobby room) => await OnRoomUpdated(room));
            lobbyManager.OnRoomLeft.AddListener(OnRoomLeft);

            ModsEvent.OnModsListUpdated += OnModListUpdated;
        }

        private void UnsubscribeLobbyEvents()
        {
            lobbyManager.OnRoomCreated.RemoveListener(OnRoomCreated);
            lobbyManager.OnJoinRequested.RemoveListener(OnLobbyJoinRequested);
            lobbyManager.OnRoomJoined.RemoveAllListeners();
            lobbyManager.OnRoomJoinFailed.RemoveListener(OnRoomJoinFailed);
            lobbyManager.OnRoomUpdated.RemoveAllListeners();
            lobbyManager.OnRoomLeft.RemoveListener(OnRoomLeft);

            ModsEvent.OnModsListUpdated -= OnModListUpdated;
        }
    }
}