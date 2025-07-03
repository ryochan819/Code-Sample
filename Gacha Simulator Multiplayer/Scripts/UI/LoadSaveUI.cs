using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Gacha.system;
using PrimeTween;
using PurrLobby;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gacha.ui
{
    public class LoadSaveUI : MonoBehaviour
    {
        [SerializeField] Button[] LoadButtons;
        [SerializeField] Button backgroundButton;
        [SerializeField] MenuButton menuButtonController;


        void Start()
        {
            backgroundButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void RefreshUI()
        {
            string[] saveNames = { "autosave", "save1", "save2", "save3" };
            string saveDirectory = Application.persistentDataPath;

            for (int i = 0; i < saveNames.Length; i++)
            {
                string saveName = saveNames[i];
                string fullPath = Path.Combine(saveDirectory, saveName + ".json");

                int index = i; // capture loop variable for lambda

                if (File.Exists(fullPath))
                {
                    Debug.Log($"Loading {saveName}...");
                    SaveData save = DataPersistenceManager.instance.Load_SaveData(fullPath);

                    if (save == null)
                    {
                        LoadButtons[i].onClick.RemoveAllListeners();
                        LoadButtons[i].GetComponent<SaveButtonUI>().SetSaveEmpty(saveName);
                        continue;
                    }

                    SaveData loadedSave = save;

                    // LoadButtons[i].GetComponent<SaveButtonUI>().SetSaveUI(
                    //     saveName,
                    //     DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Placeholder for actual save time
                    //     save.localPlayerData.level.ToString(),
                    //     save.money.ToString()
                    // );

                    LoadButtons[i].onClick.RemoveAllListeners();
                    LoadButtons[i].onClick.AddListener(() =>
                    {
                        DataPersistenceManager.instance.SaveData = loadedSave;
                        _ = menuButtonController.StartGame(false);
                        gameObject.SetActive(false);
                    });
                }
                else
                {
                    LoadButtons[i].onClick.RemoveAllListeners();
                    LoadButtons[i].GetComponent<SaveButtonUI>().SetSaveEmpty(saveName);
                }
            }
        }

        void OnEnable()
        {
            RefreshUI();
        }
    }
}