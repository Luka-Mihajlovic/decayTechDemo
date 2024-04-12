using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class controllerAnalysis : MonoBehaviour
{
    [SerializeField]
    InputActionProperty bPress = new InputActionProperty(new InputAction("SecondaryButton", expectedControlType: "Button"));
    public InputActionProperty BPress
    {
        get => bPress;
        set => SetInputActionProperty(ref bPress, value);
    }

    [SerializeField]
    GameObject leftController;
    public GameObject LeftController
    {
        get => leftController;
        set => leftController = value;
    }

    [SerializeField]
    GameObject rightController;
    public GameObject RightController
    {
        get => rightController;
        set => rightController = value;
    }

    [SerializeField]
    GameObject locomotionMove;
    public GameObject LocomotionMove
    {
        get => locomotionMove;
        set => locomotionMove = value;
    }

    float normalSpeed;

    public bool isSprinting = false;

    public static float remainingStam = 100;

    void Start()
    {
        bPress.EnableDirectAction();

        //just to make it easier to work with, this is our normal movement speed
        normalSpeed = LocomotionMove.GetComponent<forkedActionBasedContinuousMoveProvider>().moveSpeed;

        //check the position of controllers 5 times per second, then we'll use the data to implement sprinting
        
        StartCoroutine(checkControllerDelta());
    }

    IEnumerator checkControllerDelta()
    {
        int countDown = 0;
        bool checkingControllers = true;

        while (checkingControllers)
        {
            float bPressed = bPress.action.ReadValue<float>();
            float lControllerStart = leftController.transform.position.y;
            float rControllerStart = rightController.transform.position.y;

            yield return new WaitForSeconds(.2f);

            float lControllerEnd = leftController.transform.position.y;
            float rControllerEnd = rightController.transform.position.y;

            float lDelta = lControllerStart - lControllerEnd;
            float rDelta = rControllerStart - rControllerEnd;

            float stamDrain = (100 * .2f) / 2; //making it easier for calc, divide with how many seconds to last while sprinting - rn its 2
            float stamGain = stamDrain / 4; //gain 25% what you use per second, fun

            bool isStrafe = LocomotionMove.GetComponent<forkedActionBasedContinuousMoveProvider>().isStrafing;

            if (countDown == 0 || isStrafe) //check for reducing countdown and setting sprinting to the proper value
            {
                isSprinting = false;
            }

            if (bPressed == 1 && !isStrafe && remainingStam > 0) //if sprinting is even toggled + we're not strafing
            {
                if (Mathf.Abs(lDelta) > 0.2 && Mathf.Abs(rDelta) > 0.2 && (rDelta * lDelta < 0)) //big stuff but we're looking at diff direction swings + long swings
                {
                    isSprinting = true;
                    countDown = 5; //gives ~1sec (5xdelay) of coyote time for sprinting
                }
            }

            if (isSprinting)
            { 
                locomotionMove.GetComponent<forkedActionBasedContinuousMoveProvider>().moveSpeed = normalSpeed*4;
                countDown--;

                remainingStam -= stamDrain;
                if (remainingStam <= 0)
                {
                    //cleanup, swap to being non-sprinting
                    remainingStam = 1;
                    isSprinting = false;
                    locomotionMove.GetComponent<forkedActionBasedContinuousMoveProvider>().moveSpeed = normalSpeed / 2;
                    countDown = 0;

                    //wait 5s
                    yield return new WaitForSeconds(5);
                }
            }
            else
            {
                locomotionMove.GetComponent<forkedActionBasedContinuousMoveProvider>().moveSpeed = normalSpeed;
                countDown = 0;

                if (remainingStam < 100)
                {
                    remainingStam += stamGain;
                }
            }
        }
    }
    //baggage
    protected void OnEnable()
    {
        bPress.DisableDirectAction();
    }

    protected void OnDisable()
    {
        bPress.DisableDirectAction();
    }

    void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
    {
        if (Application.isPlaying)
            property.DisableDirectAction();

        property = value;

        if (Application.isPlaying && isActiveAndEnabled)
            property.EnableDirectAction();
    }
}
