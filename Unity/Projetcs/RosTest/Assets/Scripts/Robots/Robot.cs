using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot:MonoBehaviour {
    // Conexión
    public string Ip = "192.168.1.104";

    // Rangos de velocidades
    [HideInInspector]
    public int pwmRForward = 1280;
    [HideInInspector]
    public int pwmLForward = 1720;
    [HideInInspector]
    public int pwmRBackward = 1720;
    [HideInInspector]
    public int pwmLBackward = 1280;

    /* Conexion con nodos de ROS */
    [HideInInspector]
    public RosSocket rosSocket;
    [HideInInspector]
    public int movementPublisherId, resetPublisherId;

    // Velocidad de cada rueda (de 0 a 100).
    public float RVelocity = 50, LVelocity = 50, RTurnVelocity=50, LTurnVelocity=50;

    public int FOV = 15;

    // Use this for initialization
    void Awake () {
        rosSocket = new RosSocket("ws://"+Ip+":9090");
        if (rosSocket != null)
        {
            Debug.Log("Rossocket SETEADO");
        }
        else
        {
            Debug.Log("SE CAGO ROSSOCKET");
        }
        movementPublisherId = rosSocket.Advertise("movement", "std_msgs/String");
        resetPublisherId = rosSocket.Advertise("reset", "std_msgs/String");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
