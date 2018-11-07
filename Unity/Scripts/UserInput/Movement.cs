using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour {

    public bool behaviourIsRunning = false, stopped = false;
    public bool facing = false, goingToGoal = false, traceDone = false, pathObstructed = false;
    public Vector3? clickedPoint = null;


    // Variables PWM en rueda.
    public float RVelocity, LVelocity;

    private Robot robot;

    private int pwmRForward = 1280;
    private int pwmLForward = 1720;
    private int pwmRBackward = 1720;
    private int pwmLBackward = 1280;

    public NavMeshPath explorePath; // Path del que surge la explroacion
    public Vector3 proximatePoint = new Vector3(-1,-1,-1); // Punto mas proxio al robot en el path de exploracion


    // Use this for initialization
    void Start () {
        robot = gameObject.GetComponent<Robot>();
        explorePath = new NavMeshPath();
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
            if (!behaviourIsRunning)
            {
                //Debug.Log("NO BEHAVIPUR RUNING");
                Stop();
            }
                
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

    public void TurnLeft(bool notify=false)
    {
        if (notify)
        {
            stopped = false;
        }
        Send_motors_pwm(-LVelocity, RVelocity);
    }

    public void TurnRight( bool notify=false )
    {
        if (notify)
        {
            stopped = false;
        }
        
        Send_motors_pwm(LVelocity, -RVelocity);
    }

    public void GoForward(bool notify=false)
    {
        if (notify)
        {
            stopped = false;
        }
        
        Send_motors_pwm(RVelocity-7, RVelocity);
    }

    public void GoBackwards(bool notify = false)
    {
        if (notify)
        {
            stopped = false;
        }
        Send_motors_pwm(-LVelocity, -LVelocity+2);
    }

    public void Stop(bool notify=false)
    {   
        if (notify)
        {
            stopped = true;
        }

        Send_motors_pwm();
        backwards = forward = turningLeft = turningRight = false;
    }

    public void Send_motors_pwm(float left = 0, float right = 0)
    {
        int leftPWM = PWMHelper.Remap(left, -100, 100, pwmLBackward, pwmLForward);
        int rightPWM = PWMHelper.Remap(right, -100, 100, pwmRBackward, pwmRForward);

        StandardString msg = new StandardString
        {
            data = leftPWM.ToString() + "," + rightPWM.ToString()
        };
        robot.rosSocket.Publish(robot.movementPublisherId, msg);
    }
}
