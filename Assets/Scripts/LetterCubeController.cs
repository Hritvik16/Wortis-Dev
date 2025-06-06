using System;
using System.Collections;
using System.Collections.Generic;
// using System.Reflection.Emit;
using UnityEngine;
using Random = UnityEngine.Random;

public class LetterCubeController : MonoBehaviour    
{

    Dictionary<string, int> letterToPoints = new Dictionary<string, int>()
    {
        {"A", 1}, {"B", 3}, {"C", 2}, {"D", 2},
        {"E", 1}, {"F", 4}, {"G", 3}, {"H", 4},
        {"I", 1}, {"J", 8}, {"K", 5}, {"L", 1},
        {"M", 2}, {"N", 1}, {"O", 1}, {"P", 3},
        {"Q", 8}, {"R", 1}, {"S", 1}, {"T", 1},
        {"U", 1}, {"V", 4}, {"W", 4}, {"X", 10},
        {"Y", 4}, {"Z", 8}

    };

    public string Letter
    {
        get
        {
            return getLetterInFront();
        }
    }

    Dictionary<string, int> consonantLetterToIndex = new Dictionary<string, int>() {
        {"B", 1}, {"C", 2}, {"D", 3}, {"F", 5}, {"G", 6}, {"H", 7}, {"J", 9},
        {"K", 10}, {"L", 11}, {"M", 12}, {"N", 13}, {"P", 15}, {"Q", 16}, {"R", 17}, {"S", 18},
        {"T", 19}, {"V", 21}, {"W", 22}, {"X", 23}, {"Y", 24}, {"Z", 25}
    };

    Dictionary<string, int> letterToIndex = new Dictionary<string, int>()
    {
        {"A", 0}, {"B", 1}, {"C", 2}, {"D", 3}, {"E", 4}, {"F", 5}, {"G", 6}, {"H", 7},
        {"I", 8}, {"J", 9}, {"K", 10}, {"L", 11}, {"M", 12}, {"N", 13}, {"O", 14}, {"P", 15},
        {"Q", 16}, {"R", 17}, {"S", 18}, {"T", 19}, {"U", 20}, {"V", 21}, {"W", 22}, {"X", 23},
        {"Y", 24}, {"Z", 25}
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

    public Material strikeMaterial;

    public List<Texture2D > textures;

    public List<Texture2D> strikeTextures;

    public float time = 0.2f;

    public QuadInfo[] children = new QuadInfo[6];

    public Color color;

    public float targetY;
    public bool isTargetFallActive;

    bool transparent = false;


    private bool hasStarted = false;

    public bool hasRegisteredLastDownCollision = false;

    private const float settleThreshold = 0.02f;

    public bool isStruck = false;

    void Awake()
    {
        
    }

    void Start()
    {

        targetY = Mathf.Floor(transform.position.y);

        
        MeshRenderer[] faces = transform.GetComponentsInChildren<MeshRenderer>();
        
        int childIndex = 0;
        foreach (MeshRenderer face in faces)
        {
            Material mat = new Material(material);

            Material strikeMat = new Material(strikeMaterial);

            List<string> keyList = new List<string>(consonantLetterToIndex.Keys);


            string letter = keyList[UnityEngine.Random.Range(0, keyList.Count)];
            int textureIndex = consonantLetterToIndex[letter];

            if(Random.Range(1, 10) >= 6)
            {
                keyList = new List<string>(vowelLetterToIndex.Keys);
                //while(keyList.Count)
                letter = keyList[Random.Range(0, keyList.Count)];
                textureIndex = vowelLetterToIndex[letter];

                mat.SetTexture("_MainTex", textures[textureIndex]);
                strikeMat.SetTexture("_MainTex", strikeTextures[textureIndex]);

                vowelLetterToIndex.Remove(letter);

                face.material = mat;
                //face.material.SetFloat("_Mode", 3); // SET QUAD TO TRANSPARENT


                children[childIndex] = new QuadInfo(face.gameObject, letter, mat, strikeMat);

                childIndex++;
                continue;

            }
            mat.SetTexture("_MainTex", textures[textureIndex]);
            strikeMat.SetTexture("_MainTex", textures[textureIndex]);

            consonantLetterToIndex.Remove(letter);


            face.material = mat;

            children[childIndex] = new QuadInfo(face.gameObject, letter, mat, strikeMat);

            childIndex++;

        }
        // isCurrentLetterCube = true;       
        hasStarted = true;
    }

    public bool getHasStarted()
    {
        return hasStarted;
    }

    public void setTransparency(bool t)
    {
        if (transparent == t) return;
        if (t)
        {
            foreach (QuadInfo childQuadInfo in children)
            {
                //Debug.Log("yeah");
                childQuadInfo.getQuad().GetComponent<MeshRenderer>().material = childQuadInfo.getStrikeMaterial();
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
            // Debug.Log(child);
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


    // public bool CanRotate()
    // {
    //     return canRotate;
    // }

    // public void SetCanRotateToFalse()
    // {
    //     canRotate = false;
    // }

    // public void SetCanMoveToFalse()
    // {
    //     canMove = false;
    // }

    // public void SetCanMoveToTrue()
    // {
    //     canMove = true;
    // }

    // public bool CanMove()
    // {
    //     return canMove;
    // }

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

    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
    }

    public float getFallSpeed()
    {
        return fallSpeed;
    }

    public bool leftColliding() {
        Vector3 rayStartPosition = transform.position + (Vector3.down * 0.49f) + (Vector3.left * 0.49f);
        return isCollidingInDirection(rayStartPosition, Vector3.left, Color.white);
    }

    public bool rightColliding() {

        Vector3 rayStartPosition = transform.position + (Vector3.down * 0.49f) + (Vector3.right * 0.49f);
        return isCollidingInDirection(rayStartPosition, Vector3.right, Color.blue);
    }

    public bool downColliding()
    {
        
        Vector2 belowPosition = new Vector2(transform.position.x, Mathf.Floor(transform.position.y) - 1);

        // Check if floor
        if (belowPosition.y <= 0)
        {
            return true; // Floor at y = -1 or 0
        }

        // Check if another cube is already there
        return LetterCubeDataSet.SharedInstance.HasCubeAt(belowPosition);
        // Vector3 rayStartPosition = transform.position + Vector3.down * .49f;
        // Vector3 direction = Vector3.down;
        // float rayLength = Mathf.Max(0.1f, Time.deltaTime * fallSpeed + 0.05f);

        // Debug.DrawRay(rayStartPosition, direction * rayLength, color);

        // RaycastHit hit;

        // if (Physics.Raycast(rayStartPosition, direction, out hit, rayLength))
        // {
        //     Debug.DrawRay(rayStartPosition, direction * hit.distance, color);
        //     // Debug.Log(hit.collider.gameObject.name);
        //     return true;
        // }
        // Debug.DrawRay(rayStartPosition, direction * rayLength, color);
        // return false;
    }

    private bool isCollidingInDirection(Vector3 rayStartPosition, Vector3 direction, Color color)
    {
        // Vector3 rayStartPosition = transform.position + Vector3.down * .501f;
        float rayLength = 0.1f;
        Debug.DrawRay(rayStartPosition, direction * rayLength, color);
        RaycastHit hit;
      
        if (Physics.Raycast(rayStartPosition, direction, out hit, rayLength))
        {
            Debug.DrawRay(rayStartPosition, direction * hit.distance, color);
            // Debug.Log(hit.collider.gameObject.name);
            return true;
        }
        Debug.DrawRay(rayStartPosition, direction * rayLength, color);
        return false;
    }

    public void setStrikeMaterial()
    {
        int textureToIndex = letterToIndex[children[F].getLetter()];
        // Debug.Log(textureToIndex);
        // Debug.Log("Striking letter cube with letter: " + children[F].getLetter() + "strike material: " + strikeTextures[textureToIndex].name);
        StartCoroutine(FadeOutAndIn(children[F].getQuad().GetComponent<MeshRenderer>().material, strikeTextures[textureToIndex]));//, destroyAfter));
       
        isStruck = true;   
    }

    public void resetMaterial()
    {
        // Debug.Log("Unstriking letter cube with letter: " + children[F].getLetter());
        StartCoroutine(FadeOutAndIn(children[F].getQuad().GetComponent<MeshRenderer>().material, textures[letterToIndex[children[F].getLetter()]]));//, false));
       
        isStruck = false;
    }
    public void DestroyLetterCube()
    {
        // Calculate points here using front letter
        LetterCubeDataSet.SharedInstance.addPoints(letterToPoints[getLetterInFront()]);
        LetterCubeDataSet.SharedInstance.activeLetterCubes.Remove(this);//letterCube.GetComponent<LetterCubeController>());

        Destroy(gameObject);
    }
    
    IEnumerator FadeOutAndIn(Material mat, Texture newTexture)//, bool destroyAfter)
    {
        // Debug.Log(newTexture.name);
        float duration = 0.3f;
        float elapsed = 0f;

        Color tint = mat.color;

        // Fade to black (multiply to 0)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = 1.0f - t;  // goes from 1 to 0
            tint.r = tint.g = tint.b = value;
            tint.a = 1.0f;  // alpha stays full
            mat.color = tint;
            yield return null;
        }

        // Swap texture at black point
        mat.SetTexture("_MainTex", newTexture);

        // Fade back to normal (multiply to 1)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = t;  // goes from 0 to 1
            tint.r = tint.g = tint.b = value;
            tint.a = 1.0f;
            mat.color = tint;
            yield return null;
        }

        // Ensure final color is pure (1,1,1)
        mat.color = Color.white;
        // if (destroyAfter)
        // {
        //     DestroyLetterCube();
        // }
    }

    public void PerformFallStep(float? overrideFallSpeed = null)
    {
        float cappedDeltaTime = Mathf.Min(Time.deltaTime, 1f / 60f); // Clamp to ~16 ms max
        float maxStep = cappedDeltaTime * fallSpeed;

        if (isTargetFallActive)
        {
            // Gold standard fall to targetY
            float newY = Mathf.MoveTowards(transform.position.y, targetY, maxStep);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Once reached targetY AND downColliding() confirms support below
            if (Mathf.Abs(transform.position.y - targetY) < settleThreshold && downColliding())
            {
                isTargetFallActive = false;
                hasRegisteredLastDownCollision = false; // re-trigger AddLetterCube next frame
            }
        }
        else
        {
            if (!downColliding())
            {
                if (overrideFallSpeed.HasValue)
                {
                    fallSpeed = overrideFallSpeed.Value;
                }

                // Normal fall — target is grid floor
                float nextY = Mathf.Floor(transform.position.y - 1);
                float newY = Mathf.Max(transform.position.y - maxStep, nextY);

                transform.position = new Vector3(transform.position.x, newY, transform.position.z);

                hasRegisteredLastDownCollision = false;
            }
            else
            {
                // Normal settle — toward grid floor (NOT targetY here!)
                float gridTargetY = Mathf.Floor(transform.position.y);
                float currentY = transform.position.y;

                if (Mathf.Abs(currentY - gridTargetY) > settleThreshold)
                {
                    float newY = Mathf.MoveTowards(currentY, gridTargetY, maxStep);

                    transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                }
                else
                {
                    // Close enough → snap exactly to grid and register
                    transform.position = new Vector3(transform.position.x, gridTargetY, transform.position.z);
                    fallSpeed = 2f;
                    if (!hasRegisteredLastDownCollision)
                    {
                        if (!LetterCubeDataSet.SharedInstance.inUse)
                        {
                            LetterCubeDataSet.SharedInstance.AddLetterCube(new Vector2(transform.position.x, transform.position.y), transform.gameObject);
                            hasRegisteredLastDownCollision = true;
                        }
                    }
                }
            }
        }
    }


    // public void PerformFallStep(float? overrideFallSpeed = null)
    // {
    //     float cappedDeltaTime = Mathf.Min(Time.deltaTime, 1f / 60f); // Clamp to ~16 ms max
    //     float maxStep = cappedDeltaTime * fallSpeed;
    //     if (isTargetFallActive)
    //     {
    //         // Gold standard fall to targetY
    //         float newY = Mathf.MoveTowards(transform.position.y, targetY, maxStep);
    //         transform.position = new Vector3(transform.position.x, newY, transform.position.z);

    //         // Once reached targetY AND downColliding() confirms support below
    //         if (Mathf.Abs(transform.position.y - targetY) < settleThreshold && downColliding())
    //         {
    //             isTargetFallActive = false;
    //             hasRegisteredLastDownCollision = false; // re-trigger AddLetterCube next frame
    //         }
    //     }
    //     else
    //     {
    //         if (!downColliding())
    //         {
    //             if (overrideFallSpeed.HasValue)
    //             {
    //                 fallSpeed = overrideFallSpeed.Value;
    //             }
    //             // Safe step to next grid Y
    //             float nextY = Mathf.Floor(transform.position.y - 1);
    //             float newY = Mathf.Max(transform.position.y - maxStep, nextY);


    //             transform.position = new Vector3(transform.position.x, newY, transform.position.z);

    //             hasRegisteredLastDownCollision = false;
    //         }
    //         else
    //         {
    //             // targetY = Mathf.Floor(transform.position.y);
    //             float currentY = transform.position.y;

    //             // If not yet close enough → keep falling smoothly
    //             if (Mathf.Abs(currentY - targetY) > settleThreshold)
    //             {                    
    //                 float newY = Mathf.MoveTowards(currentY, targetY, maxStep);

    //                 transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    //             }
    //             else
    //             {
    //                 // Close enough → snap exactly to grid and register
    //                 transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    //                 fallSpeed = 2f;
    //                 if (!hasRegisteredLastDownCollision)
    //                 {
    //                     if (!LetterCubeDataSet.SharedInstance.inUse)
    //                     {
    //                         LetterCubeDataSet.SharedInstance.AddLetterCube(new Vector2(transform.position.x, transform.position.y), transform.gameObject);
    //                         hasRegisteredLastDownCollision = true;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

    // Update is called once per fram
    void Update()
    {

    }

}
