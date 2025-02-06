using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadInfo: MonoBehaviour
{
    GameObject quad;
    string letter;
    Material mainMaterial;
    Material transparentMaterial;

    public QuadInfo(GameObject q, string l, Material mM, Material tM)
    {
        quad = q;
        letter = l;
        mainMaterial = mM;
        transparentMaterial = tM;
    }

    public GameObject getQuad()
    {
        return quad;
    }

    public string getLetter()
    {
        return letter;
    }

    public Material getMainMaterial()
    {
        return mainMaterial;
    }

    public Material getTransparentMaterial()
    {
        return transparentMaterial;
    }
}
