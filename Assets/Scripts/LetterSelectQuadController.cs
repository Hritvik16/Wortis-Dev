using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterSelectQuadController : MonoBehaviour
{
    public GameObject gameController;
    public Material material;

    string letter;

    public void setTextureAndLetter(Texture2D texture, string l)
    {
        letter = l;

        Material mat = new Material(material);
        mat.SetTexture("_MainTex", texture);
        transform.gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    private void OnMouseOver()
    {

        if(letter != null && Input.GetButton("Fire1")) 
        {
            gameController.GetComponent<GameController>().RotateCurrentCubeToLetter(letter);
        }
        
    }
}
