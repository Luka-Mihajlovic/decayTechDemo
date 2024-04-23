using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class geigerCounter : MonoBehaviour
{
    [SerializeField]
    GameObject gCounter;

    [SerializeField]
    GameObject radioactiveSource;

    public static string radLevel;

    public AudioSource[] geigerSounds; //4=die, 3=high, 2=med, 1=low, 0=baseline

    void setSound(int target)
    {
        for (int i = 0; i < geigerSounds.Length; i++)
        {
            if (i != target)
            {
                geigerSounds[i].enabled = false;
            }
            else
            {
                geigerSounds[i].enabled = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distToSource;
        Vector3 counterPos = gCounter.transform.position;
        Vector3 sourcePos = radioactiveSource.transform.position;

        distToSource = (Mathf.Pow((counterPos.x - sourcePos.x), 2f) + Mathf.Pow((counterPos.y - sourcePos.y), 2f) + Mathf.Pow((counterPos.z - sourcePos.z), 2f));

        if (distToSource < 40f)
        {
            radLevel = "die";
            setSound(4);
        }
        else
        {
            if (distToSource < 100f) //high
            {
                radLevel = "high";
                setSound(3);
            }
            else
            {
                if (distToSource < 300f) //medium
                {
                    radLevel = "med";
                    setSound(2);
                }
                else
                {
                    if (distToSource < 500f)
                    {
                        radLevel = "low";
                        setSound(1);
                    }
                    else
                    {
                        setSound(0);
                    }
                }
            }
        }

    }
}