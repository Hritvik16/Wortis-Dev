using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public class FontTextureGenerator : EditorWindow
{
    // Settings
    public TMP_FontAsset fontAsset;
    public int textureSize = 512;
    public string outputFolder = "Assets/FontTextures";

    [MenuItem("Tools/Font Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<FontTextureGenerator>("Font Texture Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Font Texture Generator", EditorStyles.boldLabel);

        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);
        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Generate Textures"))
        {
            if (fontAsset == null)
            {
                Debug.LogError("Please assign a TMP_FontAsset!");
                return;
            }

            GenerateTextures();
        }
    }

    private void GenerateTextures()
{
    // Setup camera
    GameObject camGO = new GameObject("TempCam");
    Camera cam = camGO.AddComponent<Camera>();
    cam.clearFlags = CameraClearFlags.SolidColor;
    cam.backgroundColor = Color.clear;
    cam.orthographic = false; // Important!
    cam.fieldOfView = 10f;

    // Setup canvas
    GameObject canvasGO = new GameObject("TempCanvas");
    Canvas canvas = canvasGO.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceCamera;
    canvas.worldCamera = cam;
    canvasGO.layer = LayerMask.NameToLayer("UI");

    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
    canvasRect.sizeDelta = new Vector2(1920, 1080); // HD canvas size

    // Setup TMP object
    GameObject textGO = new GameObject("TempText");
    textGO.transform.SetParent(canvasGO.transform);
    textGO.layer = LayerMask.NameToLayer("UI");

    TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
    tmp.font = fontAsset;
    tmp.fontSize = 800;
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.color = Color.black; // or any ink color you want

    RectTransform textRect = tmp.GetComponent<RectTransform>();
    textRect.sizeDelta = new Vector2(1920, 1080);
    textRect.anchoredPosition = Vector2.zero;

    // Create render texture
    RenderTexture rt = new RenderTexture(textureSize, textureSize, 24);
    cam.targetTexture = rt;

    // Create output folder if needed
    if (!Directory.Exists(outputFolder))
    {
        Directory.CreateDirectory(outputFolder);
    }

    // Loop through letters A–Z
    for (char c = 'A'; c <= 'Z'; c++)
    {
        tmp.text = c.ToString();
        tmp.ForceMeshUpdate();

        // Wait 1 frame to ensure UI is updated (Editor coroutine not needed)
        cam.Render();

        // Save to texture
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        tex.Apply();

        // Save PNG
        byte[] bytes = tex.EncodeToPNG();
        string filePath = Path.Combine(outputFolder, $"{c}.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Saved: {filePath}");

        DestroyImmediate(tex);
    }

    // Cleanup
    RenderTexture.active = null;
    cam.targetTexture = null;
    DestroyImmediate(rt);
    DestroyImmediate(camGO);
    DestroyImmediate(canvasGO);

    AssetDatabase.Refresh();
    Debug.Log("Font texture generation complete!");
}

}
