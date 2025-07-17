using System.Collections.Generic;
using Gacha.system;
using UnityEngine;

public class MaterialOverrideHandler
{
    public List<MaterialCache> materialCacheList;
    public List<MaterialCache> CacheOriginalMaterials(GameObject root)
    {
        materialCacheList = new List<MaterialCache>();
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(false))
        {
            if (renderer.enabled)
            {
                materialCacheList.Add(new MaterialCache(renderer));
            }
        }

        return materialCacheList;
    }

    public void ApplyOverrideMaterial(bool valid)
    {
        Material overrideMaterial = valid ? GameReference.Instance.ValidPlacementMaterial : GameReference.Instance.InvalidPlacementMaterial;
        
        foreach (var cache in materialCacheList)
        {
            var mats = new Material[cache.originalMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = overrideMaterial;

            cache.renderer.materials = mats;
        }
    }

    public void RestoreOriginalMaterials()
    {
        foreach (var cache in materialCacheList)
        {
            cache.Restore();
        }
        ResetHandler();
    }

    public void ResetHandler()
    {
        materialCacheList.Clear();
    }
}
