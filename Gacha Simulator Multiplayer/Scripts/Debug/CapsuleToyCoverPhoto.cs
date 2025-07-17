#if UNITY_EDITOR
using System.IO;
using Gacha.gameplay;
using UnityEditor;
using UnityEngine;

public class CapsuleToyCoverPhoto : MonoBehaviour
{
    public PrizeData prizeData;
    void Start()
    {
        TakeScreenShot();
    }
    
    void TakeScreenShot()
    {
        string assetPath = AssetDatabase.GetAssetPath(prizeData);
        string folderPath = Path.GetDirectoryName(assetPath);
        string fileName = prizeData.setName + ".png";
        string fullPath = Path.Combine(folderPath, fileName);

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log("Screenshot saved to: " + fullPath);
    }
}
#endif