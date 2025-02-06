using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WordLoader : MonoBehaviour
{
    public static HashSet<string> LoadWords()
    {
        string filePath = "filtered_words";
        HashSet<string> wordSet = new HashSet<string>();

        TextAsset textAsset = Resources.Load<TextAsset>(filePath);

        if (textAsset != null)
        {
            string[] words = textAsset.text.Split('\n'); // Split by newlines

            foreach (string word in words)
            {
                string trimmedWord = word.Trim(); // Remove any extra spaces or newlines
                if (!string.IsNullOrEmpty(trimmedWord)) // Skip empty lines
                {
                    wordSet.Add(trimmedWord); // Add to HashSet
                }
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        return wordSet;
    }
}
