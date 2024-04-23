using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticUI : MonoBehaviour
{
    public Image uiImage;
    private Color originalColor;

    [SerializeField]
    private GameObject radTarget;

    Renderer radRender;

    void Start()
    {
        originalColor = uiImage.color;
        radRender = radTarget.GetComponent<Renderer>();
    }

    void Update()
    {
        float alpha;
        if (radRender.isVisible)
        {
            string radLevel = geigerCounter.radLevel;
            switch (radLevel)
            {
                case "die":
                    alpha = 0.75f;
                    break;
                case "high":
                    alpha = 0.5f;
                    break;
                case "med":
                    alpha = 0.25f;
                    break;
                default:
                    alpha = 0f;
                    break;
            }
        }
        else
        {
            alpha = 0f;
        }

        uiImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}