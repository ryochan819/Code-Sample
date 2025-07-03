using System.IO;
using System.Threading.Tasks;
using Gacha.gameplay;
using UnityEditor;
using UnityEngine;

public class PrizePhoto : MonoBehaviour
{
    public PrizeData prizeData;
    public GameObject[] prizePrefab; // The prefab to instantiate for the prize
    public PrizeType prizeType;
    public Transform photoPosition;

    async void Start()
    {
        foreach (GameObject prefab in prizePrefab)
        {
            if (prefab != null)
            {
                await TakeScreenShot(prefab);
            }
        }
    }

    async Task TakeScreenShot(GameObject prefab)
    {
        Vector3 originalPosition = prefab.transform.position;
        prefab.transform.position = photoPosition.position; // Move the prefab to the photo position

        // Define the file path and name
        // string folderPath = Application.dataPath + "/" + prizeType + "/" + prizeData.setName + "/";
        string assetPath = AssetDatabase.GetAssetPath(prizeData);
        string folderPath = Path.GetDirectoryName(assetPath);
        Debug.Log("Folder Path: " + folderPath);
        string fileName = prefab.name + ".png";
        string fullPath = Path.Combine(folderPath, fileName);

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Capture and save the screenshot
        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log("Screenshot saved to: " + fullPath);
        await Task.Delay(500);
        prefab.transform.position = originalPosition; // Reset the prefab position
    }

    public enum PrizeType
    {
        CapsuleToy,
    }
}
