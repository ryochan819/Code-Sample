using System.IO;
using Cysharp.Threading.Tasks;
using Gacha.system;
using UnityEngine;

public class FileDataHandler
{
    string path;
    bool useEncryption = false;
    private readonly string encryptionCodeWord = "LuCkyDrAw2025";

    public FileDataHandler(string dataDirPath)
    {
        path = dataDirPath;
    }

    public T LoadJsonData<T>() where T : class
    {
        T loadedData = null;

        if (File.Exists(path))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (typeof(T) == typeof(SaveData) && useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadedData = JsonUtility.FromJson<T>(dataToLoad);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error occurred when trying to load data from file: " + path + "\n" + e);
            }
        }

        return loadedData;
    }

    public async UniTask<T> LoadJsonDataAsync<T>() where T : class
    {
        T loadedData = null;

        if (File.Exists(path))
        {
            try
            {
                string dataToLoad = "";

                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                using (StreamReader reader = new StreamReader(stream))
                {
                    dataToLoad = await reader.ReadToEndAsync();
                }

                if (typeof(T) == typeof(SaveData) && useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadedData = JsonUtility.FromJson<T>(dataToLoad);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error occurred when trying to asynchronously load data from file: " + path + "\n" + e);
            }
        }

        return loadedData;
    }

    public void Save(SaveData data)
    {
        try
        {
            // create the directory the file will be weitten to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // serialize the C# game data object into Jason
            string dataToStore = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write te serialized data to the file
            // use using to ensure connection to the save file is closed after reading or writing
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
            Debug.Log("Saved");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + path + "\n" + e);
        }
    }

    public async UniTask SaveAsync(SaveData data)
    {
        try
        {
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Serialize the C# game data object into JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // Write the serialized data to the file asynchronously
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(dataToStore);
            }

            Debug.Log("Saved (async)");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occurred when trying to asynchronously save data to file: " + path + "\n" + e);
        }
    }

    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }
}

