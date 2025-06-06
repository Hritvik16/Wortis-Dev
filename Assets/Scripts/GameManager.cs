using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Private variables
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

    private HashSet<string> validWordSet;

    private GameObject currentLetterCube;

    private Vector2 touchStartPos;
    
    private Vector2 touchEndPos;

    private int minXSpawnPoint = -2;

    private int maxXSpawnPoint = 3;

    private int maxRowLength = 5;

    private int ySpawnPoint = 13;

    private int zSpawnPoint = 0;

    private float swipeThreshold = 50f;

    private bool gameOver = false;

    private bool isSettling = false;

    private bool pendingSettle = false;

    // Public variables
    public GameObject gameOverTextField;

    public GameObject diffcultySelector;

    public GameObject letterCube;

    public GameObject[] letterSelectQuads;

    public GameObject scoreText;
    // Start is called before the first frame update
    void Start()
    {
        gameOverTextField.active = false;
        diffcultySelector.active = false;
        validWordSet = WordLoader.LoadWords();
        InstantiateLetterCube();
    }

  

     void InstantiateLetterCube()
    {
        Vector3 InstantiatePosition = new Vector3(Random.Range(minXSpawnPoint, maxXSpawnPoint), ySpawnPoint, zSpawnPoint);
        Quaternion InstantiateQuaternion = new Quaternion(0f, 0f, 0f, 0f);
        currentLetterCube = Instantiate(letterCube, InstantiatePosition, InstantiateQuaternion);


        LetterCubeDataSet.SharedInstance.activeLetterCubes.Add(currentLetterCube.GetComponent<LetterCubeController>());

        while (!currentLetterCube.activeInHierarchy) { }

        // UpdateLetterSelectQuads(currentLetterCube);
        StartCoroutine(RunFunctionAfterFrameEnds(() => UpdateLetterSelectQuads(currentLetterCube)));
        // Debug.Log("We here");
        // StartCoroutine(UpdateLetterCubesAfterFrameEnds());  
    }
    IEnumerator RunFunctionAfterFrameEnds(Action func)
    {
         while (currentLetterCube != null && currentLetterCube.GetComponent<LetterCubeController>() != null && !currentLetterCube.GetComponent<LetterCubeController>().getHasStarted())
        {
            yield return null;
        }

        // 🚩 If currentLetterCube got destroyed, skip calling func
        if (currentLetterCube == null || currentLetterCube.GetComponent<LetterCubeController>() == null)
        {
            yield break; // Exit coroutine safely
        }

        func();
        // while (!currentLetterCube.GetComponent<LetterCubeController>().getHasStarted()) yield return null;
        // func();
    }
    void UpdateLetterSelectQuads(GameObject _letterCube)
    {
        // Get the letter from the current letter cube
        int i = 0;
        string[] letters = _letterCube.GetComponent<LetterCubeController>().getLetters();
        Texture2D[] textures = _letterCube.GetComponent<LetterCubeController>().getTextures();

        foreach (GameObject letterSelectQuad in letterSelectQuads)
        {
            string letter = letters[i];
            Texture2D texture = textures[i];
            letterSelectQuad.GetComponent<LetterSelectQuadController>().setTextureAndLetter(texture, letter);

            i++;
        }
    }

    void HandleInput()
    {
        float direction = 0;   

        bool SetFallSpeed = Input.GetKeyUp(KeyCode.Space);
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                float swipeDistanceY = touchEndPos.y - touchStartPos.y;

                // Check if the swipe is downward
                if (swipeDistanceY < -swipeThreshold)
                {
                    //Debug.Log("Down swipe detected!");
                    SetFallSpeed = true;
                    // currentLetterCube.GetComponent<LetterCubeController>().SetFallSpeed(20f);
                }
                else if (swipeDistanceY > swipeThreshold)
                {
                    //Debug.Log("Down swipe detected!");
                    SetFallSpeed = true;
                    // currentLetterCube.GetComponent<LetterCubeController>().SetFallSpeed(20f);
                }
                else
                {

                    Ray ray = Camera.main.ScreenPointToRay(touch.position); // Create a ray from touch position

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.tag.Equals("letterSelectQuad"))
                        {
                            string letter = hit.collider.gameObject.GetComponent<LetterSelectQuadController>().getLetter();
                            currentLetterCube.GetComponent<LetterCubeController>().RotateTo(letter);
                        }
                        else if (hit.collider.tag.Equals("letter_cube") && hit.collider.gameObject != currentLetterCube)
                        {
                            isSettling = true;
                            LetterCubeDataSet.SharedInstance.inUse = true;
                            LetterCubeDataSet.SharedInstance.RemoveLetterCubesFrom(hit.collider.transform.position);
                            pendingSettle = true;
                        }
                        else
                        {
                            direction = Camera.main.ScreenToWorldPoint(touch.position).x - currentLetterCube.transform.position.x;
                        }

                    }
                    else
                    {
                        direction = Camera.main.ScreenToWorldPoint(touch.position).x - currentLetterCube.transform.position.x;
                    }
                }
            }
        }
        if (SetFallSpeed)
        {
            currentLetterCube.GetComponent<LetterCubeController>().SetFallSpeed(20f);
        }
        // Move left
        if (Input.GetKeyUp(KeyCode.A) || direction < 0)
        {

            if (!currentLetterCube.GetComponent<LetterCubeController>().leftColliding())
            {
                currentLetterCube.transform.position = new Vector3(currentLetterCube.transform.position.x - 1, currentLetterCube.transform.position.y, currentLetterCube.transform.position.z);
            }
        }

        // Move right
        if (Input.GetKeyUp(KeyCode.D) || direction > 0)
        {

            if (!currentLetterCube.GetComponent<LetterCubeController>().rightColliding())
            {
                currentLetterCube.transform.position = new Vector3(currentLetterCube.transform.position.x + 1, currentLetterCube.transform.position.y, currentLetterCube.transform.position.z);
            }
        }
    }

    void HandleLetterCubeFall()
    {
        LetterCubeController[] cubes = GameObject.FindObjectsOfType<LetterCubeController>();

        LetterCubeController[] orderedCubes = LetterCubeDataSet.SharedInstance.activeLetterCubes
        .OrderBy(cube => cube.transform.position.y)
        .ToArray();


        bool allSettled = true;

        foreach (LetterCubeController cube in orderedCubes)
        {
            // 🚩 For stage cubes → force fallSpeed = 2f
            if (cube.gameObject != currentLetterCube)
            {
                cube.PerformFallStep(5f);
            }
            else
            {
                // 🚩 Let currentLetterCube fall with its own speed
                cube.PerformFallStep();
            }

            if (!cube.downColliding())
            {
                allSettled = false;
            }
        }

        if (allSettled)
        {
            isSettling = false;
            LetterCubeDataSet.SharedInstance.inUse = false;
        }
    }

    void HandleScoreDisplay()
    {
        int score = LetterCubeDataSet.SharedInstance.GetScore();
        scoreText.GetComponent<TMP_Text>().text = "Score: " + score.ToString();
    }

    void HandleStageUpdate()
    {
        if (pendingSettle)
        {
            // 🚩 Wait one frame before resuming fall loop
            pendingSettle = false;
            LetterCubeDataSet.SharedInstance.inUse = false;
            return;  // Skip this frame's fall loop
        }
        if (!LetterCubeDataSet.SharedInstance.inUse)
            HandleLetterCubeFall();


        if (!isSettling && currentLetterCube.GetComponent<LetterCubeController>().downColliding())
            InstantiateLetterCube(); 
    }

    void CheckGameOver()
    {
        if (LetterCubeDataSet.SharedInstance.GetGameOver())
        {
            gameOver = true;

            currentLetterCube.GetComponent<LetterCubeController>().SetFallSpeed(0f);
            currentLetterCube.GetComponent<LetterCubeController>().StopAllCoroutines();
            LetterCubeController[] cubes = GameObject.FindObjectsOfType<LetterCubeController>();
            foreach (LetterCubeController cube in cubes)
            {
                Destroy(cube.gameObject);
            }
            gameOverTextField.active = true;
            gameOverTextField.GetComponent<TMP_Text>().text = "Game Over! \n Your score: " + LetterCubeDataSet.SharedInstance.GetScore().ToString();
            diffcultySelector.active = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            HandleInput();
            HandleScoreDisplay();
            CheckGameOver();
            // This function has to be called Last in the Update loop
            HandleStageUpdate();                                       
        }
    }
}
