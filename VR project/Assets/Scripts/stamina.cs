using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stamina : MonoBehaviour
{
    public TMP_Text canvasStamina;
    public Image canvasBackground;
    private float remainingStem;

    void Start()
    {
        canvasStamina.text = "100%";
        canvasStamina.color = Color.green;
        canvasBackground.color = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        float remainingStam = controllerAnalysis.remainingStam;

        string StringRemainingStam = remainingStam.ToString("F0") + "%";
        canvasStamina.text = StringRemainingStam;
        canvasStamina.fontStyle = FontStyles.Normal;

        canvasBackground.color = new Color(0, 0, 0, (100 - remainingStam +10) / 90f);

        if (remainingStam > 80)
        {
            canvasStamina.color = new Color(0, 255, 0);

        }
        else if (remainingStam > 50)
        {
            canvasStamina.color = new Color(255, 255, 0);
        }
        else
        {
            canvasStamina.color = new Color(255, 0, 0);
            canvasStamina.fontStyle = FontStyles.Bold;
        }

    }
}