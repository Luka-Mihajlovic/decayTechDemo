using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticUI : MonoBehaviour
{
    public Image uiImage;
    private Color originalColor;
    private string radLevel;

    void Start()
    {
        radLevel = "low";
        originalColor = uiImage.color;
    }

    void Update()
    {
        string radLevel = geigerCounter.radLevel;
        Debug.Log(radLevel);
        float alpha;
        switch (radLevel)
        {
            case "die":
                alpha = 0.3f; 
                break;
            case "high":
                alpha = 0.2f;
                break;
            case "med":
                alpha = 0.1f;
                break;
            default:
                alpha = 0f;
                break;
        }
        uiImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}