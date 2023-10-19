using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ch.sycoforge.Decal;

public class ItemLogo : MonoBehaviour
{
    [SerializeField]
    EasyDecal _EasyDecal;

    public void SetDecal(Material material)
    {
        _EasyDecal.DecalMaterial = material;
    }
}
