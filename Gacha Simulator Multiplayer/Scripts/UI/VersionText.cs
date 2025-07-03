using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = Application.version;
    }
}
