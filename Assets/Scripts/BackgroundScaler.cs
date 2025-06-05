using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        float height = Camera.main.orthographicSize * 3f;
        float width = height * Camera.main.aspect;

        float size = height > width ? height : width;
        transform.localScale = new Vector3(size, size, 1f);
    }
}
