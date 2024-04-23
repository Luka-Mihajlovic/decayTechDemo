//Attach this script to a GameObject with a Renderer component attached
//If the GameObject is visible to the camera, the message is output to the console

using UnityEngine;

public class IsVisible : MonoBehaviour
{
    Renderer m_Renderer;
    // Use this for initialization
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Renderer.isVisible)
        {
            Debug.Log("Object is visible");
        }
        else Debug.Log("Object is no longer visible");
    }
}