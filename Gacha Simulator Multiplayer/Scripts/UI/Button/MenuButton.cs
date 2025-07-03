using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Gacha.system;
using PurrLobby;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PrimeTween;

public class MenuButton : MonoBehaviour
{
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] MenuLobbyManager menuLobbyManager;
    [SerializeField] Button continueButton;
    [SerializeField] Button newGameButton;
    [SerializeField] Button modPanelButton;
    [SerializeField] Button quitButton;

    [SerializeField] GameObject loadPanel;
    [SerializeField] GameObject modPanel;
    [PurrScene, SerializeField] string nextScene;
    [SerializeField] Image loadingBackground;

    bool repeatBlock = false;
    
    private void Start() {
        if (DemoCheck.IsDemo)
        {
            modPanelButton.gameObject.SetActive(false);
        }
    }

    public async Task StartGame(bool newGame)
    {
        if (repeatBlock) return;
        repeatBlock = true;

        if (newGame)
        DataPersistenceManager.instance.InitializeGameData();

        if (!SteamManager.Initialized || !lobbyManager.CurrentLobby.IsValid)
        {
            Debug.Log("Single player game");
            GameManager.isMultiplayer = false;
            loadingBackground.raycastTarget = true;
            await LoadingFadeIn();
            await SceneManager.LoadSceneAsync(nextScene);
            repeatBlock = false;
            return;
        }

        // Check if mods ready and has lobby and all players are ready, if no lobby start game
        if (lobbyManager.CurrentLobby.IsValid)
        {
            GameManager.isMultiplayer = true;
            bool isAllReady = await lobbyManager.CheckAllReadyButHostAsync();
            if (isAllReady)
            {
                Debug.Log("All players are ready");
                lobbyManager.ToggleLocalReady();
                loadingBackground.raycastTarget = true;
                await menuLobbyManager.LoadScene(true);
            }
            else
            {
                // UI to notice player

                Debug.Log("Waiting for other players to be ready");
            }
        }

        repeatBlock = false;
    }

    private void SetupButtons(bool set)
    {
        if (set)
        {
            continueButton.onClick.AddListener(() =>
            {
                loadPanel.SetActive(true);
            });

            newGameButton.onClick.AddListener(() => {
                _ = StartGame(true);
            });

            modPanelButton.onClick.AddListener(() => {
                modPanel.SetActive(!modPanel.activeSelf);
            });

            quitButton.onClick.AddListener(() => {
                Application.Quit();
            });
        }
        else
        {
            newGameButton.onClick.RemoveAllListeners();
            modPanelButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }
    }

    async UniTask LoadingFadeIn()
    {
        var tween = Tween.Custom(
            0f, 1f,
            duration: 0.3f,
            onValueChange: a =>
            {
                var c = loadingBackground.color;
                c.a = a;
                loadingBackground.color = c;
            }
        );

        await tween.ToYieldInstruction(); // GC-free, avoids struct boxing
    }

    private void OnEnable() {
        SetupButtons(true);
    }

    void OnDisable()
    {
        SetupButtons(false);
    }
}
