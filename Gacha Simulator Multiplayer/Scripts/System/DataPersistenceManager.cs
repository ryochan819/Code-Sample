using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gacha.system
{
    public class DataPersistenceManager : MonoBehaviour
    {
        public static DataPersistenceManager instance { get; private set; }

        [Header("File Storage Config")]
        [SerializeField] private bool useEncryption;
        public bool useEncryptionAccess => useEncryption;

        SaveData saveData;
        public SaveData SaveData { get => saveData; set => saveData = value; }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(this); }
            else { instance = this; }

            DontDestroyOnLoad(gameObject);

            InitializeGameData();
        }

        public void InitializeGameData()
        {
            saveData = new SaveData();
        }

        public SaveData Load_SaveData(string path)
        {
            FileDataHandler fileDataHandler = new FileDataHandler(path);
            saveData = fileDataHandler.LoadJsonData<SaveData>();
            return saveData;
        }

        public async UniTask<SaveData> Load_SaveDataAsync(string path)
        {
            FileDataHandler fileDataHandler = new FileDataHandler(path);
            saveData = await fileDataHandler.LoadJsonDataAsync<SaveData>();
            return saveData;
        }

        public async UniTask Save_SaveDataAsync(string path)
        {
            await GatherRequiredSaveData(path);
        }

        public async UniTask GatherRequiredSaveData(string path)
        {
            // Get Data Reference from Game Scene Manager
            await GameSceneDataManager.instance.Save_GameData();
            FileDataHandler fileDataHandler = new FileDataHandler(path);
            await fileDataHandler.SaveAsync(saveData);
        }
    }
}
