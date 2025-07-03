using System.Collections;
using System.Collections.Generic;
using CC;
using Gacha.system;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Gacha.gameplay
{
    public static class Utility
    {
        public static Transform GetDeepChild(Transform parent, string name)
        {
            Transform result = parent.Find(name);
            if (result != null)
                return result;
            foreach (Transform child in parent)
            {
                result = GetDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static Color HexToColor(string hex)
        {
            Color color = Color.white;
            hex = hex.Contains("#") ? hex : "#" + hex;

            if (ColorUtility.TryParseHtmlString(hex, out color))
            {
                return color;
            }
            return Color.white;
        }

        public static T FindChildWithComponent<T>(Transform parent) where T : Component
        {
            foreach (Transform child in parent)
            {
                // Check if the child has the desired component
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;

                // Recursive search in the child's children
                component = FindChildWithComponent<T>(child);
                if (component != null)
                    return component;
            }

            // Return null if the component is not found
            Debug.Log("Not found");
            return null;
        }

        public static List<T> FindAllChildrenWithComponent<T>(Transform parent) where T : Component
        {
            List<T> components = new List<T>();

            foreach (Transform child in parent)
            {
                // Check if the child has the desired component
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }

                // Recursive search in the child's children
                components.AddRange(FindAllChildrenWithComponent<T>(child));
            }

            return components;
        }

        public static string GetLocalString(string key, string table = "Localization")
        {
            var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
            if (stringOperation.Contains("No translation found"))
            {
                return key;
            }
            return stringOperation;
        }

        public static float SignedAngleTo(this Vector2 a, Vector2 b)
        {
            return Mathf.Atan2(a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y) * Mathf.Rad2Deg;
        }

        public static CC_CharacterDataNetwork ConvertToNetworkData(CC_CharacterData characterData)
        {
            List<CC_PropertyNetwork> ConvertPropertyList(List<CC_Property> sourceList)
            {
                if (sourceList == null) return new List<CC_PropertyNetwork>();

                var converted = new List<CC_PropertyNetwork>(sourceList.Count);
                foreach (var prop in sourceList)
                {
                    converted.Add(new CC_PropertyNetwork
                    {
                        propertyName = prop.propertyName,
                        stringValue = prop.stringValue,
                        floatValue = prop.floatValue,
                        colorValue = prop.colorValue,
                        materialIndex = prop.materialIndex,
                        meshTag = prop.meshTag
                    });
                }
                return converted;
            }

            return new CC_CharacterDataNetwork
            {
                CharacterName = characterData.CharacterName,
                CharacterPrefab = characterData.CharacterPrefab,
                Blendshapes = ConvertPropertyList(characterData.Blendshapes),
                HairNames = new List<string>(characterData.HairNames),
                ApparelNames = new List<string>(characterData.ApparelNames),
                ApparelMaterials = new List<int>(characterData.ApparelMaterials),
                FloatProperties = ConvertPropertyList(characterData.FloatProperties),
                TextureProperties = ConvertPropertyList(characterData.TextureProperties),
                ColorProperties = ConvertPropertyList(characterData.ColorProperties)
            };
        }

        public static CC_CharacterData ConvertToLocalData(CC_CharacterDataNetwork networkData)
        {
            List<CC_Property> ConvertPropertyList(List<CC_PropertyNetwork> sourceList)
            {
                if (sourceList == null) return new List<CC_Property>();

                var converted = new List<CC_Property>(sourceList.Count);
                foreach (var prop in sourceList)
                {
                    converted.Add(new CC_Property
                    {
                        propertyName = prop.propertyName,
                        stringValue = prop.stringValue,
                        floatValue = prop.floatValue,
                        colorValue = prop.colorValue,
                        materialIndex = prop.materialIndex,
                        meshTag = prop.meshTag
                    });
                }
                return converted;
            }

            return new CC_CharacterData
            {
                CharacterName = networkData.CharacterName,
                CharacterPrefab = networkData.CharacterPrefab,
                Blendshapes = ConvertPropertyList(networkData.Blendshapes),
                HairNames = new List<string>(networkData.HairNames),
                ApparelNames = new List<string>(networkData.ApparelNames),
                ApparelMaterials = new List<int>(networkData.ApparelMaterials),
                FloatProperties = ConvertPropertyList(networkData.FloatProperties),
                TextureProperties = ConvertPropertyList(networkData.TextureProperties),
                ColorProperties = ConvertPropertyList(networkData.ColorProperties)
            };
        }

        public static IEnumerator BounceToPosition(Transform target, Vector3 position, Quaternion rotation, float duration = 0.5f, float bounceHeight = 0.5f)
        {
            if (target == null) yield break;

            Vector3 startPosition = target.position;
            Quaternion startRotation = target.rotation;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                // Bounce effect
                float bounce = Mathf.Sin(t * Mathf.PI) * bounceHeight;
                Vector3 currentPosition = Vector3.Lerp(startPosition, position, t) + new Vector3(0, bounce, 0);
                Quaternion currentRotation = Quaternion.Slerp(startRotation, rotation, t);

                target.position = currentPosition;
                target.rotation = currentRotation;

                yield return null;
            }

            // Ensure final position and rotation are set
            target.position = position;
            target.rotation = rotation;
        }

        public static void ApplyValidMaterial(GameObject obj, bool valid)
        {
            var targetMaterial = valid ? GameReference.Instance.ValidPlacementMaterial : GameReference.Instance.InvalidPlacementMaterial;

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                if (renderer.enabled)
                {
                    renderer.material = targetMaterial;
                }
            }
        }
    }
}
