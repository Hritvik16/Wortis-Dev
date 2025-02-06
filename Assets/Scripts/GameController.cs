
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    //int[] points = new int[] { 1, 3, 3, 2, 12, 2, 8, 8, 1, 8, 5, 1, 2, 1, 1, 3, 8, 1, 1, 1, 4, 4, 4, 10, 4, 8 };

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

    public int totalPoints = 0;

    public float fallSpeed = 1f;

    public int minX = 0;
    public int maxX = 0;

    public int y = 0;

    public GameObject[] letterSelectQuads;

    public GameObject gameOverTextField;

    public GameObject particleSystem;

    private int ySpawnPoint = 13;
    private int zSpawnPoint = 0;

    private int minXSpawnPoint = -2;
    private int maxXSpawnPoint = 3;

    public GameObject letterCube;

    public GameObject pointsTextField;

    public GameObject buttons;
    GameObject currentLetterCube;


    int maxRowLength = 5;
    int maxColumnLength = 7;
    //GameObject[,] cubeGrid = new GameObject[5, 7];

    int[] lowestOpenColumnIndexes = new int[5] { 0, 0, 0, 0, 0 };

    Dictionary<int, HashSet<GameObject>> wantedIndexToCube = new Dictionary<int, HashSet<GameObject>>();

    Dictionary<int, HashSet<int>> indicesAtRow = new Dictionary<int, HashSet<int>>();

    Dictionary<string, GameObject> coordsToSetCubes = new Dictionary<string, GameObject>();

    bool gameStateSet = true;

    private int instantiateCount = 0;

    int deadWordIndex = -1;

    bool requestWaiting = false;

    bool gameOver = false;

    List<string> wordIndicesSet = new List<string>();

    private static int minimumValidWordLength = 5 ;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float swipeThreshold = 50f; // Minimum distance for a swipe to be detected

    private HashSet<string> validWordSet;
    // Start is called before the first frame update
    void Start()
    {
        gameOverTextField.active = false;
        buttons.active = false;
        validWordSet = WordLoader.LoadWords();
        InstantiateLetterCube();
    }

    void InstantiateLetterCube()
    {
        instantiateCount++;

        Vector3 InstantiatePosition = new Vector3(Random.Range(minXSpawnPoint, maxXSpawnPoint), ySpawnPoint, zSpawnPoint);
        Quaternion InstantiateQuaternion = new Quaternion(0f, 0f, 0f, 0f);
        currentLetterCube = Instantiate(letterCube, InstantiatePosition, InstantiateQuaternion);

        StartCoroutine(UpdateLetterCubesAfterFrameEnds());
        
    }

    IEnumerator UpdateLetterCubesAfterFrameEnds()

    {
        while (!currentLetterCube.activeInHierarchy || !currentLetterCube.GetComponent<LetterCubeController>().hasStarted) yield return null;

        int i = 0;
        string[] letters = currentLetterCube.GetComponent<LetterCubeController>().getLetters();
        Texture2D[] textures = currentLetterCube.GetComponent<LetterCubeController>().getTextures();

        foreach (GameObject letterSelectQuad in letterSelectQuads)
        {
            string letter = letters[i];
            Texture2D texture = textures[i];
            letterSelectQuad.GetComponent<LetterSelectQuadController>().setTextureAndLetter(texture, letter);

            i++;
        }

        // Code to execute on the next frame

    }

    public void RotateCurrentCubeToLetter(string letter)
    {
        currentLetterCube.GetComponent<LetterCubeController>().RotateTo(letter);
    }

    

    void RemoveCubeRow(int startIncl, int endExcl, int y)
    {
        //Debug.Log(startIncl + " " + endExcl);       
        wordIndicesSet = new List<string>();
        for (int x = startIncl; x < endExcl; x++)
        {
            //Debug.Log(x + " " + y);
            GameObject cube = coordsToSetCubes[x + "" + y];
            //cubeGrid[x, y] = null;

            //Debug.Log(cubeGrid[x, y] == null);
            //Debug.Log("Removing: " + x + "" + y + coordsToSetCubes[x + "" + y].GetComponent<LetterCubeController>().getLetterInFront());
            coordsToSetCubes.Remove(x + "" + y);

            Destroy(Instantiate(particleSystem, cube.transform.position, cube.transform.rotation), 0.25f);
            Destroy(cube);
            MoveAllCubesOneRowDown(x, y + 1);

            lowestOpenColumnIndexes[x] -= 1;
        }
        ResetTransparencyOfAllCubes();
        //turnValidWordCubesTransparent();
    }

    void MoveAllCubesOneRowDown(int x, int from)
    {

        // Instead of below logic, try scouring through coordsToSetCubes[x + "" + y[0->7]]
        // 
        for (int y = from; y < maxColumnLength; y++)
        {
            if(!coordsToSetCubes.ContainsKey(x + "" + y)) {
                break;
            }

            coordsToSetCubes.Add(x + "" + (int)(y - 1), coordsToSetCubes[x + "" + y]);

            coordsToSetCubes[x + "" + y].GetComponent<LetterCubeController>().setTransparency(false);

            if (wantedIndexToCube.ContainsKey(y - 1))
            {
                wantedIndexToCube[y - 1].Add(coordsToSetCubes[x +  "" + (int)(y - 1) ]);
            }
            else
            {
                wantedIndexToCube.Add(y - 1, new HashSet<GameObject>(new GameObject[] { coordsToSetCubes[x + "" + (int)(y - 1)] }));
            }

            coordsToSetCubes.Remove(x + "" + y);

            //break;

            //if (cubeGrid[x, y] == null)
            //{
            //    if (coordsToSetCubes.ContainsKey(x + "" + (int)(y - 1)))
            //    {
            //        coordsToSetCubes.Remove(x + "" + (y - 1));
            //    }
            //    break;
            //}               

            //LetterCubeController letterCubeController;
            
            //if (cubeGrid[x, y] != null) {
                //int prevY = y - 1;
                //Debug.Log(x + " " + y + " T");
                //coordsToSetCubes[x + "" + prevY] = cubeGrid[x, y - 1];
                //coordsToSetCubes[x + "" + prevY].GetComponent<LetterCubeController>().setTransparency(false);
                //if (wantedIndexToCube.ContainsKey(y - 1))
                //{
                //    wantedIndexToCube[y - 1].Add(cubeGrid[x, y - 1]);
                //}
                //else
                //{
                //    wantedIndexToCube.Add(y - 1, new HashSet<GameObject>(new GameObject[] { cubeGrid[x, y - 1] }));
                //}
                //if(y == 6)
                //{
                //    coordsToSetCubes.Remove(x + "" + y);
                //}

        }
    }

    void SolveWantedIndices()
    {
        
        HashSet<int> keysToRemove = new HashSet<int>();

        if (keysToRemove.Count > 0) gameStateSet = false;

        foreach (int y in wantedIndexToCube.Keys)
        {
            if (wantedIndexToCube[y].Count == 0)
            {
                keysToRemove.Add(y);
            }
            HashSet<GameObject> cubesToRemove = new HashSet<GameObject>();
            foreach (GameObject cube in wantedIndexToCube[y])
            {
                int currentYIndex = (int)(Mathf.Ceil(cube.transform.position.y) - 1);
                if (currentYIndex == y)
                {
                    cubesToRemove.Add(cube);
                }
                else
                {
                    cube.transform.Translate(Vector3.down * Time.deltaTime * fallSpeed * 1.5f, Space.World);
                }
                //Debug.Log(y + " " + currentYIndex);
            }
            foreach (GameObject cube in cubesToRemove)
            {
                wantedIndexToCube[y].Remove(cube);
            }
        }

        foreach (int key in keysToRemove)
        {
            wantedIndexToCube.Remove(key);
        }

        
    }

    void HandleInput()
    {
        int currentXIndex = (int)(currentLetterCube.transform.position.x + 2);
        int currentYIndex = (int)(Mathf.Ceil(currentLetterCube.transform.position.y) - 1);

        float direction = 0;

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
                float swipeDistance = touchEndPos.y - touchStartPos.y;

                // Check if the swipe is downward
                if (swipeDistance < -swipeThreshold)
                {
                    //Debug.Log("Down swipe detected!");
                    fallSpeed = 20f;
                    // Handle the downward swipe action here
                }
                else
                {

                    Ray ray = Camera.main.ScreenPointToRay(touch.position); // Create a ray from touch position

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))

                    {
                        if (!hit.collider.tag.Equals("letterSelectQuad"))
                        {
                            direction = Camera.main.ScreenToWorldPoint(touch.position).x - currentLetterCube.transform.position.x;
                        }
                        // "hit.collider.gameObject" is the object that was touched

                        //Debug.Log("Touched object: " + hit.collider.gameObject.name);

                    }
                    else
                    {
                        direction = Camera.main.ScreenToWorldPoint(touch.position).x - currentLetterCube.transform.position.x;
                    }
                }
            }
        }

        // Move left
        if (Input.GetKeyUp(KeyCode.A) || direction < 0)
        {

            if (currentXIndex > 0 && lowestOpenColumnIndexes[currentXIndex - 1] < currentYIndex)
            {
                currentLetterCube.transform.position = new Vector3(currentLetterCube.transform.position.x - 1, currentLetterCube.transform.position.y, currentLetterCube.transform.position.z);
            }
        }

        // Move right
        if (Input.GetKeyUp(KeyCode.D) || direction > 0)
        {

            if (currentXIndex < maxRowLength - 1 && lowestOpenColumnIndexes[currentXIndex + 1] < currentYIndex)
            {
                currentLetterCube.transform.position = new Vector3(currentLetterCube.transform.position.x + 1, currentLetterCube.transform.position.y, currentLetterCube.transform.position.z);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {

            SolveWantedIndices();

            int currentXIndex = (int)(currentLetterCube.transform.position.x + 2);
            int currentYIndex = (int)(Mathf.Ceil(currentLetterCube.transform.position.y) - 1);

            HandleInput();


            if (currentYIndex == lowestOpenColumnIndexes[currentXIndex])
            {
                lowestOpenColumnIndexes[currentXIndex] += 1;
                currentLetterCube.GetComponent<LetterCubeController>().SetCanRotateToFalse();
                currentLetterCube.GetComponent<LetterCubeController>().SetCanMoveToFalse();

                if (currentYIndex >= maxColumnLength)
                {
                    gameOver = true;
                    gameOverTextField.active = true;
                    return;
                }

                //cubeGrid[currentXIndex, currentYIndex] = currentLetterCube;
                if(coordsToSetCubes.ContainsKey(currentXIndex + "" + currentYIndex))
                {
                    Debug.Log(currentXIndex + "" + currentYIndex + coordsToSetCubes[currentXIndex + "" + currentYIndex].GetComponent<LetterCubeController>().getLetterInFront());
                }
                coordsToSetCubes.Add(currentXIndex + "" + currentYIndex, currentLetterCube);//cubeGrid[currentXIndex, currentYIndex]);

                turnValidWordCubesTransparent();

                if (indicesAtRow.ContainsKey(currentYIndex))
                {
                    indicesAtRow[currentYIndex].Add(currentXIndex);
                }
                else
                {
                    indicesAtRow.Add(currentYIndex, new HashSet<int>() { currentXIndex });
                }
                fallSpeed = 1f;
                currentLetterCube.transform.position = new Vector3(currentXIndex - 2, currentYIndex + 1, currentLetterCube.transform.position.z);

                InstantiateLetterCube();
            }

            currentLetterCube.gameObject.transform.Translate(Vector3.down * Time.deltaTime * fallSpeed, Space.World);


            // CALCULATE SCORE / REMOVING CUBES HERE
            if (wantedIndexToCube.Keys.Count == 0 && instantiateCount > 5)
            {
                //Debug.Log("Here");
                FindAndRemoveValidMaxRowLengthLetterWords();
                
            }
            if(wantedIndexToCube.Keys.Count == 0 && gameStateSet == false)
            {
                gameStateSet = true;
                turnValidWordCubesTransparent();
            }
        }
        else
        {
            buttons.active = true;
        }
        pointsTextField.GetComponent<TMP_Text>().text = "points: " + totalPoints;                
    }

    public void setMinimumValidWordLength(int wordLength)
    {
        if (gameOver)
        {
            minimumValidWordLength = wordLength;
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    void ResetTransparencyOfAllCubes()
    {
        foreach (string key in coordsToSetCubes.Keys)
        {
            coordsToSetCubes[key].GetComponent<LetterCubeController>().setTransparency(false);
        }
    }

    public void TransparentLetterCubeClicked(LetterCubeController letterCubeController)
    {
        foreach (string key in coordsToSetCubes.Keys)
        {
            if(coordsToSetCubes[key] == letterCubeController.transform.gameObject)
            {
                int xIndex = int.Parse(key[0] + "");
                int yIndex = int.Parse(key[1] + "");

                List<string> wordIndicesToAdd = new List<string>();
                int maxWordLength = -1;
                foreach (string wordIndices in wordIndicesSet)
                {
                    Debug.Log(wordIndices);
                    string[] indices = wordIndices.Split(",");
                    //Debug.Log(wordIndices);
                    //Debug.Log(indices);
                    foreach (string index in indices)
                    {
                       
                        int xI = int.Parse(index[0] + "");
                        int yI = int.Parse(index[1] + "");

                        if(yI != yIndex)
                        {
                            break;
                        }

                        if(xI == xIndex)
                        {
                            wordIndicesToAdd.Add(wordIndices);
                            if (wordIndices.Length > maxWordLength)
                                maxWordLength = wordIndices.Length;

                            break;
                        }
                    }
                }

                List<string> wordsToConsider = new List<string>();
                foreach (string wordIndices in wordIndicesToAdd)
                {
                    if(wordIndices.Length == maxWordLength)
                    {
                        wordsToConsider.Add(wordIndices);
                    }
                }
                Debug.Log(wordsToConsider.Count + " " + maxWordLength);
                string wordIndex = wordsToConsider[Random.Range(0, wordsToConsider.Count)];
                string[] i = wordIndex.Split(",");

                int startingIndex = int.Parse(i[0][0] + "");
                int endingIndex = int.Parse(i[i.Length - 1][0] + "");

                for(int x = startingIndex; x <= endingIndex; x++)
                {
                    //Debug.Log(x);
                    totalPoints += letterToPoints[coordsToSetCubes[x + "" + yIndex].GetComponent<LetterCubeController>().getLetterInFront()];
                }
                //Debug.Log(wordIndex + " " + startingIndex + " " + endingIndex + " " + yIndex);
                RemoveCubeRow(startingIndex, endingIndex + 1, yIndex);


                //TODO: UPDATE POINTS

                break;
            }
        }
    }

    void turnValidWordCubesTransparent()
    {        
        foreach (string coords in coordsToSetCubes.Keys)
        {
            //Debug.Log(coords);
            int coordX = int.Parse(coords[0] + "");
            int coordY = int.Parse(coords[1] + "");

            //Debug.Log(coords);

            string word = coordsToSetCubes[coords].GetComponent<LetterCubeController>().getLetterInFront();
            List<LetterCubeController> letterCubes = new List<LetterCubeController>() { coordsToSetCubes[coords].GetComponent<LetterCubeController>() }
;            for (int i = coordX + 1; i < coordX + 5; i++)
             {

                string coordsXi = i + "" + coordY;
                if (!coordsToSetCubes.ContainsKey(coordsXi)) break;
                //if (coordsToSetCubes[coordsXi].GetComponent<LetterCubeController>().isTransparent()) break;

                word += coordsToSetCubes[coordsXi].GetComponent<LetterCubeController>().getLetterInFront();
                letterCubes.Add(coordsToSetCubes[coordsXi].GetComponent<LetterCubeController>());

                if (word.Length >= minimumValidWordLength)
                {

                    if(validWordSet.Contains(word.ToLower()))
                    {
                        int x = i;

                        string _coords = "";
                        for (int j = x; j > x - word.Length; j--)
                        {
                            if (j == x)
                            {
                                _coords = j + "" + coordY;
                                continue;
                            }
                            _coords = j + "" + coordY + "," + _coords;
                        }
                        Debug.Log(_coords);
                        wordIndicesSet.Add(_coords);
                        foreach (LetterCubeController letterCube in letterCubes)
                        {
                            letterCube.setTransparency(true);
                        }
                    }
                }
            }
        }
    }


    void FindAndRemoveValidMaxRowLengthLetterWords()
    {
        for (int j = deadWordIndex + 1; j < 7; j++)
        {
            int tempPoints = 0;
            string word = "";
            bool check = false;
            for (int i = 0; i < maxRowLength; i++)
            {
                if (!coordsToSetCubes.ContainsKey(i+ "" +j)) { check = true; break; };
                word += coordsToSetCubes[i + "" + j].GetComponent<LetterCubeController>().getLetterInFront();
                tempPoints += letterToPoints[coordsToSetCubes[i + "" + j].GetComponent<LetterCubeController>().getLetterInFront()];
            }

            if (check) break;

            if (word.Length == maxRowLength)
            {

                if (validWordSet.Contains(word.ToLower()))
                {

                    totalPoints += tempPoints;

                    RemoveCubeRow(0, maxRowLength, j);

                }
                    
            }
        }
    }

}
