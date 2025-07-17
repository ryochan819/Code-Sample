#if UNITY_EDITOR
using System.IO;
using System.Threading.Tasks;
using Gacha.gameplay;
using UnityEditor;
using UnityEngine;

public class PrizePhoto : MonoBehaviour
{
    public PrizeData prizeData;
    public GameObject[] prizePrefab;
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
        prefab.transform.position = photoPosition.position;

        string assetPath = AssetDatabase.GetAssetPath(prizeData);
        string folderPath = Path.GetDirectoryName(assetPath);
        Debug.Log("Folder Path: " + folderPath);
        string fileName = prefab.name + ".png";
        string fullPath = Path.Combine(folderPath, fileName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log("Screenshot saved to: " + fullPath);
        await Task.Delay(500);
        prefab.transform.position = originalPosition;
    }

    public enum PrizeType
    {
        CapsuleToy,
    }
}
#endif