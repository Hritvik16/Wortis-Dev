using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadInfo: MonoBehaviour
{
    GameObject quad;
    string letter;
    Material mainMaterial;
    Material strikeMaterial;

    public QuadInfo(GameObject q, string l, Material mM, Material tM)
    {
        quad = q;
        letter = l;
        mainMaterial = mM;
        strikeMaterial = tM;
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

    public Material getStrikeMaterial()
    {
        return strikeMaterial;
    }
}
