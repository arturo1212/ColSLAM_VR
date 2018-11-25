using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawControl : MonoBehaviour {
    Robot robot;
    public GameObject controller;
    ControllerData control;
    public bool holding = false, shouldHold = false, shouldLook = false;
    int horizontalAxis;
    
    void Start()
    {
        robot = GetComponent<Robot>();
        control = controller.GetComponent<ControllerData>();
    }


    private void sendAction(int action)
    {
        StandardString msg = new StandardString();
        msg.data = action.ToString();
        robot.rosSocket.Publish(robot.clawPublisherId, msg);
    }

    void Update () {
        if (control.triggerPressed)
        {
            shouldHold = true;
        }

        if(control.triggerPressUp && shouldHold)
        {
            holding = !holding;
            sendAction(holding ? 0 : 1);
            shouldHold = false;
        }

        if (control.touchPadTouched)
        {
            shouldLook = true;
            horizontalAxis = PWMHelper.Remap(control.touchPad.x, -1, 1, 55, 130);
        }

        if(!control.touchPadTouched && shouldLook)
        {
            sendAction(horizontalAxis);
            shouldLook = false;
        }
    }
}
