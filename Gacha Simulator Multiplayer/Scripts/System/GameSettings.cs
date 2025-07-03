using System;
using System.IO;
using Steamworks;
using UnityEngine;

namespace Gacha.system
{
    public class GameSettings : MonoBehaviour
    {
        string settingDataPath;
        void Start()
        {
            settingDataPath = Application.persistentDataPath + "/GameSettings.json";
            if (SteamManager.Initialized)
            {
                string steamLanguage = SteamApps.GetCurrentGameLanguage();
                Debug.Log("Steam Language: " + steamLanguage);
            }
            GameSetup();
        }

        private void GameSetup()
        {
            GameSettingData data;

            if (File.Exists(settingDataPath))
            {
                string json = File.ReadAllText(settingDataPath);
                data = JsonUtility.FromJson<GameSettingData>(json);
                Debug.Log("Loaded game settings.");
            }
            else
            {
                data = new GameSettingData(); // default settings
                if (SteamManager.Initialized)
                {
                    data.language = SteamApps.GetCurrentGameLanguage();
                }
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(settingDataPath, json);
            }

            ApplySettings(data);
        }

        private void ApplySettings(GameSettingData data)
        {
            Screen.fullScreen = data.fullscreen;
        }
    }

    public class GameSettingData
    {
        public bool fullscreen = true;
        public string language = "english";
    }
}