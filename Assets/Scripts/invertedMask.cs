using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InvertedMask : Mask
{
    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        Material material = baseMaterial;
        if (graphic != null)
        {
            material = Instantiate(baseMaterial);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
        }
        return material;
    }
}