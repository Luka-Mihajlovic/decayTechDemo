using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class keyDoor : MonoBehaviour
{
    [SerializeField]
    private GameObject lockedDoor;

    [SerializeField]
    private AudioSource unlockSound;

    private bool isUsed = false;

    private void Start()
    {
        lockedDoor.GetComponent<Rigidbody>().isKinematic = true; //makes the object kinematic, no physics
        lockedDoor.GetComponent<XRGrabInteractable>().enabled = false; //object cannot be interacted with, grabbed
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == lockedDoor && !isUsed)
        {
            lockedDoor.GetComponent<Rigidbody>().isKinematic = false; //object is affected by physics now, aka unlocked
            lockedDoor.GetComponent<XRGrabInteractable>().enabled = true; //object can be interacted with, grabbed
            Debug.Log("Door unlocked");

            unlockSound.Play();
            isUsed = true;
        }
    }
}
