using System.Collections.Generic;
using UnityEngine;

public class MaterialOverrideHandler
{
    private List<MaterialCache> materialCacheList = new();

    public void CacheOriginalMaterials(GameObject root)
    {
        materialCacheList.Clear();
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled)
            {
                materialCacheList.Add(new MaterialCache(renderer));
            }
        }
    }

    public void ApplyOverrideMaterial(Material overrideMat)
    {
        foreach (var cache in materialCacheList)
        {
            var mats = new Material[cache.originalMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = overrideMat;

            cache.renderer.materials = mats;
        }
    }

    public void RestoreOriginalMaterials()
    {
        foreach (var cache in materialCacheList)
        {
            cache.Restore();
        }
    }

    public void ResetHandler()
    {
        materialCacheList.Clear();
    }
}
