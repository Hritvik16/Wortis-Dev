using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectDifficulty : MonoBehaviour
{

    public int validWordLength;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                Debug.Log("Touch end logged");
                Ray ray = Camera.main.ScreenPointToRay(touch.position);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == transform.gameObject)//.Equals("difficulty_selector"))
                    {
                        Debug.Log("Attempting to set difficulty to " + validWordLength);
                        GameSettings.Instance.StartingMinimumWordLength = validWordLength;
                        Debug.Log("Difficulty set to " + GameSettings.Instance.StartingMinimumWordLength);
                        // LetterCubeDataSet.StartingMinimumWordLength = validwordLength;
                        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

                    }
                }
            }
        }

    }
}
