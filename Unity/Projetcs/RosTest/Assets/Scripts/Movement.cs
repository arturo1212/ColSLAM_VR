using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    // Variables de ROS
    RosSocket rosSocket;
    public string robotIP = "ws://192.168.1.105:9090";  // IP del robot.
    public string Topic = "movement";
    public int publicationId;

    // Variables PWM en rueda.
    public int pwmRForward = 1280;
    public int pwmLForward = 1720;
    public int pwmRBackward = 1720;
    public int pwmLBackward = 1280;

    public bool facing = false;


    // Use this for initialization
    void Start () {
        rosSocket = new RosSocket(robotIP);
        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            GoForward();
            //print("W key was pressed");
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            TurnLeft();
            //print("A key was pressed");
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            GoBackwards();
            //print("S key was pressed");
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            TurnRight();
            //print("D key was pressed");
        }
        else
        {
            //print("Stop");
            if(!facing)
            Stop();
        }
    }

    public void TurnLeft()
    {
        send_motors_pwm(pwmLBackward, pwmRForward);
    }

    public void TurnRight()
    {
        send_motors_pwm(pwmLForward, pwmRBackward);
    }

    public void GoForward()
    {
        send_motors_pwm(pwmLForward, pwmRForward);
    }

    public void GoBackwards()
    {
        send_motors_pwm(pwmLBackward, pwmRBackward);
    }

    public void Stop()
    {
        send_motors_pwm();
    }

    void send_motors_pwm(float left = 0, float right = 0)
    {
        StandardString msg = new StandardString
        {
            data = left.ToString() + "," + right.ToString()
        };
        rosSocket.Publish(publicationId, msg);
    }
}
