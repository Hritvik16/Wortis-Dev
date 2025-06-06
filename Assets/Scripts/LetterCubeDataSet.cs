using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class LetterCubeDataSet : MonoBehaviour
{

    public static LetterCubeDataSet SharedInstance { get; private set; }

    public class LetterCubeData
    {

        public int startX;

        public int endX;

        public GameObject letterCube;

        public string longestWordPossible;

        public string Letter
        {
            get
            {
                return letterCube.GetComponent<LetterCubeController>().getLetterInFront();
            }
        }
        public override string ToString()
        {
            return $"LetterCube: {letterCube}, LongestWordPossible: {longestWordPossible}";
        }
    }
    // Private variables

    Dictionary<Vector2, LetterCubeData> letterCubeDataSet = new Dictionary<Vector2, LetterCubeData>();

    private HashSet<string> validWordSet;
    
    private int minXSpawnPoint = -2;
    private int maxXSpawnPoint = 2;

    public bool inUse = false;

    // private Dictionary<int, int> columnToHeight = new Dictionary<int, int>(){
    //         {-2, 0},
    //         {-1, 0},
    //         {0, 0},
    //         {1, 0},
    //         {2, 0}
    //     };

    private int totalPoints = 0;

    private bool gameOver = false;

    // public static int StartingMinimumWordLength = 5;
    public int minimumValidLength = 3;
    // private int minimumValidLength = 5;

    private void Awake()
    {
        // if (SharedInstance == null)
        // {
        //     SharedInstance = this;
        //     DontDestroyOnLoad(gameObject);
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
        SharedInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        validWordSet = WordLoader.LoadWords();
        minimumValidLength = GameSettings.Instance.StartingMinimumWordLength;
        letterCubeDataSet.Clear();
        // Debug.Log(validWordSet.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public (bool, int) isBeyondAvailableSpot(int x, float y, GameObject letterCube)
    // {
    //     if (letterCubeDataSet.ContainsKey(new Vector2(x, y)) && letterCubeDataSet[new Vector2(x, y)].letterCube == letterCube)
    //     {
    //         // Debug.Log($"Letter cube at position {new Vector2(x, y)} already exists with a different letter cube. Returning.");
    //         return (false, -1);
    //     }
        
    //     return ((int)y <= columnToHeight[x], columnToHeight[x] + 1);  
    // }
  
    
    // public int getFirstAvailableY(int x)
    // {
    //     int y = 1;

    //     while (letterCubeDataSet.ContainsKey(new Vector2(x, y)))
    //     {
    //         y++;
    //     }
    //     return y;        
    // }


    public bool HasCubeAt(Vector2 position)
    {
        return letterCubeDataSet.ContainsKey(position);
    }
    
    public void addPoints(int points)
    {
        totalPoints += points;
    }

    public int GetScore()
    {
        return totalPoints;
    }

    public bool GetGameOver()
    {
        return gameOver;
    }

    public void StrikeAndUnstrikeWords(int y)
    {
        // STEP 1: Find the longest word in this row
        string overallLongestWord = "";
        int startXOfLongestWord = -10;

        for (int i = minXSpawnPoint; i <= maxXSpawnPoint; i++)
        {
            if (letterCubeDataSet.ContainsKey(new Vector2(i, y)))
            {
                string word = letterCubeDataSet[new Vector2(i, y)].letterCube.GetComponent<LetterCubeController>().getLetterInFront();
                string longestValidWordAtI = "";

                for (int j = i + 1; j <= maxXSpawnPoint; j++)
                {
                    if (letterCubeDataSet.ContainsKey(new Vector2(j, y)))
                    {
                        word += letterCubeDataSet[new Vector2(j, y)].letterCube.GetComponent<LetterCubeController>().getLetterInFront();
                        word = word.ToLower();

                        if (validWordSet.Contains(word))
                        {
                            // Debug.Log(word);
                            longestValidWordAtI = word;
                        }

                        continue;
                    }

                    break;
                }

                // If this word is longer than the current overall longest, update
                if (longestValidWordAtI.Length > overallLongestWord.Length)
                {
                    overallLongestWord = longestValidWordAtI;
                    startXOfLongestWord = i;
                }
            }
        }

        // STEP 2: Go through the row again → strike or unstrike as needed
        for (int i = minXSpawnPoint; i <= maxXSpawnPoint; i++)
        {
            Vector2 pos = new Vector2(i, y);

            if (letterCubeDataSet.ContainsKey(pos))
            {
                bool shouldBeStruck = false;

                if (overallLongestWord.Length >= minimumValidLength && startXOfLongestWord >= minXSpawnPoint)
                {
                    if (i >= startXOfLongestWord && i < startXOfLongestWord + overallLongestWord.Length)
                    {
                        shouldBeStruck = true;
                    }
                }

                if (shouldBeStruck)
                {
                    if (!letterCubeDataSet[pos].letterCube.GetComponent<LetterCubeController>().isStruck)
                    {
                        // Debug.Log($"Striking letter cube at {pos} with longest word: {overallLongestWord}");
                        // Strike
                        letterCubeDataSet[pos].letterCube.GetComponent<LetterCubeController>().setStrikeMaterial();
                    }

                    // Set longestWordPossible and start/end range
                    letterCubeDataSet[pos].longestWordPossible = overallLongestWord;
                    letterCubeDataSet[pos].startX = startXOfLongestWord;
                    letterCubeDataSet[pos].endX = startXOfLongestWord + overallLongestWord.Length - 1;
                }
                else
                {
                    if (letterCubeDataSet[pos].letterCube.GetComponent<LetterCubeController>().isStruck)
                    {
                        // Debug.Log($"Unstriking letter cube at {pos} with longest word: {overallLongestWord}");
                        // Unstrike
                        letterCubeDataSet[pos].letterCube.GetComponent<LetterCubeController>().resetMaterial();
                    }
                    // Clear longestWordPossible and reset start/end range
                    letterCubeDataSet[pos].longestWordPossible = "";
                    letterCubeDataSet[pos].startX = -10;
                    letterCubeDataSet[pos].endX = -10;
                }
            }
        }
        if (overallLongestWord.Length == 5)
        {
            StartCoroutine(DelayedRemovalOfRowAt(y));
        }
    }
    
    public void AddLetterCube(Vector2 position, GameObject letterCube)
    {
        if(position. y > 7) gameOver = true;
        // SANITY CHEKS
        if (position.y - 1 > 0 && !letterCubeDataSet.ContainsKey(new Vector2(position.x, position.y - 1)))
        {
            Debug.Log($"Adding a cube at {position} where there is nothing below {position}!!");
        }

        if (letterCubeDataSet.ContainsKey(position))
        {
            Debug.Log($"Letter cube with letter {letterCube.GetComponent<LetterCubeController>().Letter} at position {position} already exists. Returning.");
            // position.y = getFirstAvailableY((int)position.x);
            // letterCube.transform.position = new Vector3(position.x, position.y, letterCube.transform.position.z);

            return;
        }

        int y = (int)position.y;

        // Add the new letter cube to the data set
        LetterCubeData letterCubeData = new LetterCubeData();
        letterCubeData.letterCube = letterCube;
        letterCubeData.longestWordPossible = "";
        letterCubeDataSet.Add(position, letterCubeData);

        // columnToHeight[(int)position.x] = y;
        StrikeAndUnstrikeWords(y);

    }

    IEnumerator DelayedRemovalOfRowAt(int y)
    {
        yield return new WaitForSeconds(0.62f);
        RemoveLetterCubesFrom(new Vector2(-2, y));
    }

    public void RemoveLetterCubesFrom(Vector2 position)
    {
        if (letterCubeDataSet[position].longestWordPossible.Length < 2)
        {
            return;
        }

        int startX = letterCubeDataSet[position].startX;
        int startY = (int)position.y;
        int maxX = letterCubeDataSet[position].endX;

        int maxY = 7;

        List<GameObject> letterCubesToDestroy = new List<GameObject>();

        for (int x = startX; x <= maxX; x++)
        {
            // We will destroy the cube at (x, startY) AFTER clearing keys in the column
            letterCubesToDestroy.Add(letterCubeDataSet[new Vector2(x, startY)].letterCube);

            // Clear all keys in this column from startY up
            for (int y = startY; y <= maxY; y++)
            {
                Vector2 pos = new Vector2(x, y);

                if (letterCubeDataSet.ContainsKey(pos))
                {
                    // Save the cube to destroy (only for startY)


                    // Remove from dictionary
                    letterCubeDataSet.Remove(pos);
                }
                else
                {
                    break;
                }
            }
        }

        foreach (GameObject letterCube in letterCubesToDestroy)
        {
            // Destroy(letterCube);
            letterCube.GetComponent<LetterCubeController>().DestroyLetterCube();
            // columnToHeight[(int)letterCube.transform.position.x] = startY - 1;
        }

    }
}


