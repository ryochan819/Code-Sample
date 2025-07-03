using UnityEngine;

[CreateAssetMenu(fileName = "ModScriptable", menuName = "Scriptable Objects/ModScriptable")]
public class ModScriptable : ScriptableObject
{
    public string modName;
    public GameObject prefab;
}
