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

    public bool facing = false, turningRight = false, turningLeft = false, forward = false, backwards = false, stopped = false;
    public Vector3? clickedPoint = null;


    // Variables PWM en rueda.
    public float RVelocity, LVelocity;

    [HideInInspector]
    private int pwmRForward = 1280;
    private int pwmLForward = 1720;
    private int pwmRBackward = 1720;
    private int pwmLBackward = 1280;




    // Use this for initialization
    void Start () {
        rosSocket = new RosSocket(robotIP);
        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
    }

    // Update is called once per frame

    public void WASD()
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
            if (!facing)
                Stop();
        }
    }

    Vector3? GetClickedPoint()
    {
        /* Obtener punto seleccionado */
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        int layer_mask = LayerMask.GetMask("Floor");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {
            if (hit.collider.gameObject.name != "Piso")
            {
                Debug.Log("NO HAY NADA");
                return null;
            }
            //Vector3 point  = transform.InverseTransformPoint(hit.point);
            Debug.Log("PISO CLICKEADO");
            return hit.point;
        }
        return null;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickedPoint = GetClickedPoint();
        }
        WASD();
    }

    public void TurnLeft()
    {
        turningLeft = true;
        stopped = false;
        send_motors_pwm(-LVelocity, RVelocity);
    }

    public void TurnRight()
    {
        turningRight = true;
        stopped = false;
        send_motors_pwm(LVelocity, -RVelocity);
    }

    public void GoForward()
    {
        forward = true;
        stopped = false;
        send_motors_pwm(RVelocity-7, RVelocity);
    }

    public void GoBackwards()
    {
        backwards = true;
        stopped = false;
        send_motors_pwm(-LVelocity, -LVelocity+2);
    }

    public void Stop()
    {
        stopped = true;
        send_motors_pwm();
        backwards = forward = turningLeft = turningRight = false;
    }

    public void send_motors_pwm(float left = 0, float right = 0)
    {
        int leftPWM = PWMHelper.Remap(left, -100, 100, pwmLBackward, pwmLForward);
        int rightPWM = PWMHelper.Remap(right, -100, 100, pwmRBackward, pwmRForward);

        StandardString msg = new StandardString
        {
            data = leftPWM.ToString() + "," + rightPWM.ToString()
        };
        rosSocket.Publish(publicationId, msg);
    }
}
