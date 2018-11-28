using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;

    public bool behaviourIsRunning = false, stopped = false, debug = true, enableControl = true;
    public bool facing = false, goingToGoal = false, traceDone = false, pathObstructed = false, tooFar = false, prision = false, arrivedGreen = false;
    public Vector3 metaPoint;
    public Vector3? greenPoint = null;

    [HideInInspector]
    public Robot robot;

    private int pwmRForward = 1280;
    private int pwmLForward = 1720;
    private int pwmRBackward = 1720;
    private int pwmLBackward = 1280;

    public NavMeshPath explorePath; // Path del que surge la explroacion
    public Vector3 proximatePoint = new Vector3(-1,-1,-1); // Punto mas proxio al robot en el path de exploracion
    public float greenArrive, wanderSphereRadius=1, wanderDistance;

    public List<GameObject> metaPoints;
    [HideInInspector]
    public NaiveMapping naiv;
    public int counter = 0;
    [HideInInspector]
    int index = 0;
    public GameObject controller;
    ControllerData control;
    // Use this for initialization
    void Awake () {
        robot = gameObject.GetComponent<Robot>();
        explorePath = new NavMeshPath();
        naiv = GetComponent<NaiveMapping>();
        metaPoints = MapMerge.GetChildObject(transform.parent, "MetaPoints");
    }

    private void Start()
    {
        control = controller.GetComponent<ControllerData>();
    }

    public void WASD()
    {

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || (control.touchPadPressed && control.touchPad.y > 0.7f))
        {
            GoForward();
            //print("W key was pressed");
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || (control.touchPadPressed && control.touchPad.x < -0.7f))
        {
            TurnLeft();
            //print("A key was pressed");
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || (control.touchPadPressed && control.touchPad.y < -0.7f))
        {
            GoBackwards();
            //print("S key was pressed");
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (control.touchPadPressed && control.touchPad.x > 0.7f))
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

    public void calculateMetaPoint()
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
            Vector3 point = Random.insideUnitSphere * wanderSphereRadius;
            point += transform.position + transform.forward * (wanderDistance / naiv.scale);
            NavMeshHit navhit;
            if (NavMesh.SamplePosition(point, out navhit, wanderSphereRadius + 0.5f, NavMesh.AllAreas)) {
                metaPoint = navhit.position;
                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //cube.transform.position = metaPoint;
            }
            //metaPoint = metaPoints[index].transform.position;
        }

        Debug.DrawLine(transform.position, metaPoint, Color.red, 10);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //metaPoint = GetClickedPoint();
        }
        if (enableControl)
        {
            WASD();
        }
        //if (stopped)
       //{
        //    Send_motors_pwm();
        //}
        /*if (counter == 0)
        {
            index++;
            index = index % metaPoints.Count;
            counter++;
        }
        counter = counter % 5;
        calculateMetaPoint();
        */
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
        //Debug.Log("Enviando PWM");
        robot.movementPublisherId = robot.rosSocket.Advertise("movement", "std_msgs/String");
        robot.rosSocket.Publish(robot.movementPublisherId, msg);
    }
}
