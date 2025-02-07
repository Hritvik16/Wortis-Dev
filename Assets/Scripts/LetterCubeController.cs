using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterCubeController : MonoBehaviour    
{

    Dictionary<string, int> consonantLetterToIndex = new Dictionary<string, int>() {
        {"B", 1}, {"C", 2}, {"D", 3}, {"F", 5}, {"G", 6}, {"H", 7}, {"J", 9},
        {"K", 10}, {"L", 11}, {"M", 12}, {"N", 13}, {"P", 15}, {"Q", 16}, {"R", 17}, {"S", 18},
        {"T", 19}, {"V", 21}, {"W", 22}, {"X", 23}, {"Y", 24}, {"Z", 25}
    };

    Dictionary<string, int> vowelLetterToIndex = new Dictionary<string, int>()
    {
        {"A", 0}, {"E", 4}, {"I", 8}, {"O", 14}, {"U", 20}
    };
    const int F = 0;
    const int B = 1;
    const int L = 2;
    const int R = 3;
    const int U = 4;
    const int D = 5;

    public float fallSpeed = 1f;
    public Material material;

    public Material transparentMaterial;

    public List<Texture2D > textures;

    public float time = 0.2f;

    public QuadInfo[] children = new QuadInfo[6];

    private bool canRotate = true;
    private bool canMove = true;

    private bool isCurrentLetterCube;

    public Color color;

    bool transparent = false;


    public bool hasStarted = true;
    


    void Start()
    {
        
        MeshRenderer[] faces = transform.GetComponentsInChildren<MeshRenderer>();
        
        int childIndex = 0;
        foreach (MeshRenderer face in faces)
        {
            Material mat = new Material(material);

            Material transparentMat = new Material(transparentMaterial);

            List<string> keyList = new List<string>(consonantLetterToIndex.Keys);


            string letter = keyList[Random.Range(0, keyList.Count)];
            int textureIndex = consonantLetterToIndex[letter];

            if(Random.Range(1, 10) >= 6)
            {
                keyList = new List<string>(vowelLetterToIndex.Keys);
                //while(keyList.Count)
                letter = keyList[Random.Range(0, keyList.Count)];
                textureIndex = vowelLetterToIndex[letter];

                mat.SetTexture("_MainTex", textures[textureIndex]);
                transparentMat.SetTexture("_MainTex", textures[textureIndex]);

                vowelLetterToIndex.Remove(letter);

                face.material = mat;
                //face.material.SetFloat("_Mode", 3); // SET QUAD TO TRANSPARENT


                children[childIndex] = new QuadInfo(face.gameObject, letter, mat, transparentMat);

                childIndex++;
                continue;

            }
            mat.SetTexture("_MainTex", textures[textureIndex]);
            transparentMat.SetTexture("_MainTex", textures[textureIndex]);

            consonantLetterToIndex.Remove(letter);


            face.material = mat;

            children[childIndex] = new QuadInfo(face.gameObject, letter, mat, transparentMat);

            childIndex++;

        }
        isCurrentLetterCube = true;       
    }

    public void setTransparency(bool t)
    {
        if (transparent == t) return;
        if (t)
        {
            foreach (QuadInfo childQuadInfo in children)
            {
                //Debug.Log("yeah");
                childQuadInfo.getQuad().GetComponent<MeshRenderer>().material = childQuadInfo.getTransparentMaterial();
                //Debug.Log(childQuadInfo.getQuad().GetComponent<MeshRenderer>().material.GetFloat("_Mode"));
                //Debug.Log(childQuadInfo.getQuad().name);
            }
        }
        else
        {
            foreach (QuadInfo childQuadInfo in children)
            {
                childQuadInfo.getQuad().GetComponent<MeshRenderer>().material = childQuadInfo.getMainMaterial();
            }
        }
        transparent = t;
    }

    public bool isTransparent()
    {
        return transparent;
    }
    public string[] getLetters()
    {
        string[] letters = new string[6];

        int i = 0;
        foreach (QuadInfo child in children)
        {
            letters[i] = child.getLetter();
            i++;
        }

        return letters;
    }

    public Texture2D[] getTextures()
    {
        Texture2D[] textures = new Texture2D[6];

        int i = 0;
        foreach (QuadInfo child in children)
        {
            textures[i] = child.getQuad().GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            i++;
        }

        return textures;
    }

    public void Rotate(Vector3 axis)
    {

        QuadTextureRotation(axis);

        transform.Rotate(axis, 90, Space.World);

        foreach (QuadInfo child in children)
        {
            //Debug.Log(child.getQuad());
            child.getQuad().transform.Rotate(Vector3.forward * (0 - child.getQuad().transform.eulerAngles.z));
        }        

    }

    public void RotateTo(string letter)
    {
        int i = 0;
        foreach (QuadInfo child in children)
        {
            //Debug.Log(child.getLetter() + " " + letter);
            //Debug.Log(child.getLetter() == letter);
            if (child.getLetter().Equals(letter))
            {
                if(i == L)
                {
                    Rotate(Vector3.down);
                }
                if (i == R)
                {
                    Rotate(Vector3.up);
                }
                if (i == U)
                {
                    Rotate(Vector3.left);
                }
                if (i == D)
                {
                    Rotate(Vector3.right);
                }
                if(i == B)
                {
                    Rotate(Vector3.left);
                    Rotate(Vector3.left);
                }
            }
            i++;
        }
    }

    void QuadCollisionDetected(string letter)
    {        
        //if(children[D].getLetter().Equals(letter))
        //{
        //    Debug.Log(letter);
        //    canRotate = false;
        //}
    }

    public bool CanRotate()
    {
        return canRotate;
    }

    public void SetCanRotateToFalse()
    {
        canRotate = false;
    }

    public void SetCanMoveToFalse()
    {
        canMove = false;
    }

    public void SetCanMoveToTrue()
    {
        canMove = true;
    }

    public bool CanMove()
    {
        return canMove;
    }

    void QuadTextureRotation(Vector3 axis)
    {
        //Debug.Log(children.Length);
        //Debug.Log(children[0]);
        // Moving up
        if (axis == Vector3.left)
        {
            QuadInfo temp = children[F];
            children[F] = children[U];
            children[U] = children[B];
            children[B] = children[D];
            children[D] = temp;

            // counter clockwise for children[L]

            //Texture2D texture = children[L].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[L].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, false));

            //// clockwise for children[R]

            //texture = children[R].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[R].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, true));           
        }

        // Moving down
        if (axis == Vector3.right)
        {
            QuadInfo tempF = children[F];
            children[F] = children[D];
            QuadInfo tempU = children[U];
            children[U] = tempF;
            QuadInfo tempB = children[B];
            children[B] = tempU;
            children[D] = tempB;


            // clockwise for children[L]

            //Texture2D texture = children[L].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[L].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, true));

            //// counter clockwise for children[R]

            //texture = children[R].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[R].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, false));
        }


        // Moving left
        if (axis == Vector3.down)
        {
            QuadInfo tempF = children[F];
            children[F] = children[L];

            QuadInfo tempB = children[B];
            children[B] = children[R];
            children[L] = tempB;

            children[R] = tempF;


            // counter clockwise for children[D]

            //Texture2D texture = children[D].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[D].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, false));

            //// clockwise for children[U]

            //texture = children[U].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[U].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, true));
        }

        // Moving right
        if (axis == Vector3.up)
        {
            QuadInfo tempF = children[F];
            children[F] = children[R];

            QuadInfo tempB = children[B];
            children[B] = children[L];
            children[L] = tempF;

            children[R] = tempB;


            //  clockwise for children[D]

            //Texture2D texture = children[D].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[D].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, true));

            //// counter clockwise for children[U]

            //texture = children[U].GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;
            //children[U].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", RotateTexture(texture, false));
        }
    }

    Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
           
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    public string getLetterInFront()
    {
        return children[F].getLetter();
    }

    // Update is called once per frame
    void Update()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.51f))
        {
            if(hit.collider.tag != "letter_cube")
            {
                fallSpeed = 0f;
            }            
                Debug.Log($"Hit: {hit.collider.tag}");                                       
        }
        transform.Translate(Vector3.down * Time.deltaTime * fallSpeed, Space.World);
        if (CanRotate())
        {
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                Rotate(Vector3.right);
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                Rotate(Vector3.left);
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                Rotate(Vector3.down);
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                Rotate(Vector3.up);
            }
        }        
    }

    private void OnMouseOver()
    {
        //Debug.Log("Mouse is over");
        //Debug.Log(GameObject.Find("GameController").GetComponent<GameController>());
        if (transparent && Input.GetButton("Fire1"))
        {
            //Debug.Log("Fired over transparent cube");
            GameObject.Find("GameController").GetComponent<GameController>().TransparentLetterCubeClicked(this);
        }
    }

}
