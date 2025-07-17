using UnityEngine;

public class MaterialCache
{
    public Renderer renderer;
    public Material[] originalMaterials;

    public MaterialCache(Renderer r)
    {
        renderer = r;
        originalMaterials = r.sharedMaterials;
    }

    public void Restore()
    {
        renderer.materials = originalMaterials;
    }
}
