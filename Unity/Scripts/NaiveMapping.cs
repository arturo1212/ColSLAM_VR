using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using System;
using UnityEngine.UI;

public class NaiveMapping : MonoBehaviour {
    private bool first_time = true;
    public float first_orientation;  // Orientacion inicial del gyroscopio.
    public float distance;          // Distancia medida entre el sharp y el lidar.
    public float rotation_robot;    // Rotacion del robot (Giroscopio).
    public float pointOrientation;   // Posicion del motor frontal. (Direccion del sensor de distancia).
    public GameObject sensorObject;
    public float sensorAngle; 

    public Vector3 tpoint;
    public int maxDistance = 500;   // Maxima distancia leida por los sensores (Se usa para escalar).
    public string robotIP = "ws://192.168.1.116:9090";  // IP del robot.

    private bool newReading = false;

    private void ReadArduino(string values)
    {
        string[] tokens = values.Split(',');
        newReading = true;
        distance = int.Parse(tokens[1]);
        rotation_robot = float.Parse(tokens[0]);
        sensorAngle = int.Parse(tokens[2]);
        pointOrientation = (rotation_robot + sensorAngle -90) ;
        Debug.Log(tokens[0]+ "   " + tokens[1] + "   " + tokens[2]);
    }

    private void subscriptionHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        ReadArduino(standardString.data);
        //Debug.Log(distance);
    }

    void Start () {
        RosSocket rosSocket = new RosSocket(robotIP);
        int subscription_id = rosSocket.Subscribe("/arduino", "std_msgs/String", subscriptionHandler);
    }

    void CreateCube()
    {
        float scaled = (distance / maxDistance) *10;
        Vector3 rotationVector = transform.rotation.eulerAngles;
        rotationVector.y = rotation_robot;                      // Asignar rotacion del gyro.
        transform.rotation = Quaternion.Euler(rotationVector);
        tpoint = new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointOrientation), 0, Mathf.Cos(Mathf.Deg2Rad * pointOrientation)) * scaled;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //cube.AddComponent<Rigidbody>();
        cube.transform.position = transform.position + tpoint;
        if (distance <= 35)
        {
            (cube.GetComponent<Renderer>()).material.color = new Color(0.5f, 1, 1);
        }
        UnityEngine.Object.Destroy(cube, 50.0f);
    }

    private void FixedUpdate()
    {
        if (newReading)
        {
            CreateCube();
            sensorObject.transform.rotation = Quaternion.AngleAxis(-sensorAngle, new Vector3(0, 1, 0));
            newReading = false;
        }
    }
}
