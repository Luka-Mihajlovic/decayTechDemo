using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class pickUpEvent : MonoBehaviour
{

    [SerializeField]
    private GameObject chaser;

    [SerializeField]
    private AudioSource siren;

    // Start is called before the first frame update
    void Start()
    {
        chaser.transform.position = new Vector3(0, 0, 0);
        chaser.GetComponent<NavMeshAgent>().enabled = false;
        chaser.GetComponent<PathFinding>().enabled = false;
    }

    public void PickupLog()
    {
        chaser.transform.position = new Vector3(-63, 10, 2); //spawn em in!!
        chaser.GetComponent<NavMeshAgent>().enabled = true;
        chaser.GetComponent<PathFinding>().enabled = true;
        siren.Play();
        this.enabled = false;
    }
}
