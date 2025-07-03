using UnityEngine;

public class ImageAssign : MonoBehaviour
{
    public GameObject targetObject;
    public Texture myTexture;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();
        var block = new MaterialPropertyBlock();
        block.SetTexture("_BaseMap", myTexture);

        meshRenderer.SetPropertyBlock(block);

        Debug.Log("Image assigned to the material.");
    }
}
