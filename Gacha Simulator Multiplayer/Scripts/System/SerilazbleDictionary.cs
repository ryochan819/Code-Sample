using System.Collections.Generic;
using CC;
using UnityEngine;

// JSON does not support Dictionary type, this script is to serialize Dictionary compatible to JSON
[System.Serializable]
public class SerilazbleDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
        {
            Debug.LogError("Deserialize a serializableDictionary, the amount of keys (" + keys.Count + ") does not match the number of values (" + values.Count + ").");
        }

        for (int i = 0; i < keys.Count; i++)
        {
            Add(keys[i], values[i]);
        }
    }
}

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}

// Json Test
[System.Serializable]
public class GameObjectData
{
    public string objectName;

    public GameObjectData(GameObject gameObject)
    {
        objectName = gameObject.name;
    }
}

[System.Serializable]
public class PlayerData
{
    public string _steamID;
    public CC_CharacterData _cc_CharacterData;

    public PlayerData(string steamID, CC_CharacterData cc_CharacterData)
    {
        _steamID = steamID;
        _cc_CharacterData = cc_CharacterData;
    }
}