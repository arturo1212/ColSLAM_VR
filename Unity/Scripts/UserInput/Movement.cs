﻿using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour {

    public bool behaviourIsRunning = false, stopped = false;
    public bool facing = false, goingToGoal = false, traceDone = false, pathObstructed = false, tooFar = false, prision = false, arrivedGreen = false;
    public Vector3? metaPoint = null;
    public Vector3? greenPoint = null;

    private Robot robot;

    private int pwmRForward = 1280;
    private int pwmLForward = 1720;
    private int pwmRBackward = 1720;
    private int pwmLBackward = 1280;

    public NavMeshPath explorePath; // Path del que surge la explroacion
    public Vector3 proximatePoint = new Vector3(-1,-1,-1); // Punto mas proxio al robot en el path de exploracion
    public float greenArrive;

    public List<GameObject> metaPoints;
    [HideInInspector]
    NaiveMapping naiv;
    public int counter = 0;
    [HideInInspector]
    int index = 0;

    // Use this for initialization
    void Start () {
        robot = gameObject.GetComponent<Robot>();
        explorePath = new NavMeshPath();
        naiv = GetComponent<NaiveMapping>();
        metaPoints = MapMerge.GetChildObject(transform.parent, "MetaPoints");
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
            if (!hit.collider.gameObject.name.Contains("Environment"))
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

    void calculateMetaPoint()
    {
        if (greenPoint != null)
        {
            //Debug.Log("GREEN POINT: "+greenPoint);
            metaPoint = (Vector3)greenPoint;
        }
        else
        {
            // Alguno
            //Debug.Log("METAPOINTS");
            metaPoint = metaPoints[index].transform.position;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            metaPoint = GetClickedPoint();
        }
        WASD();
        if (counter == 0)
        {
            index++;
            index = index % metaPoints.Count;
            counter++;
        }
        counter = counter % 5;
        calculateMetaPoint();
    }

    public void TurnLeft(bool notify=false)
    {
        if (notify)
        {
            stopped = false;
        }
        Send_motors_pwm(-robot.LTurnVelocity, robot.RTurnVelocity);
    }

    public void TurnRight( bool notify=false )
    {
        if (notify)
        {
            stopped = false;
        }
        
        Send_motors_pwm(robot.LTurnVelocity, -robot.RTurnVelocity);
    }

    public void GoForward(bool notify=false)
    {
        if (notify)
        {
            stopped = false;
        }
        
        Send_motors_pwm(robot.RVelocity -7, robot.RVelocity);
    }

    public void GoBackwards(bool notify = false)
    {
        if (notify)
        {
            stopped = false;
        }
        Send_motors_pwm(-robot.LVelocity, -robot.LVelocity +2);
    }

    public void Stop(bool notify=false)
    {   
        if (notify)
        {
            stopped = true;
        }

        Send_motors_pwm();
    }

    public void Send_motors_pwm(float left = 0, float right = 0)
    {
        if(left != 0 && right != 0)
        {
            naiv.UpdateScanNumber();
        }

        int leftPWM = PWMHelper.Remap(left, -100, 100, pwmLBackward, pwmLForward);
        int rightPWM = PWMHelper.Remap(right, -100, 100, pwmRBackward, pwmRForward);

        StandardString msg = new StandardString
        {
            data = leftPWM.ToString() + "," + rightPWM.ToString()
        };
        robot.rosSocket.Publish(robot.movementPublisherId, msg);
    }
}
