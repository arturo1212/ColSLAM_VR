using UnityEngine;
using RosSharp.RosBridgeClient;

public class PorfavorFunciona : MonoBehaviour {
    public float memesCalientes;
    private bool first_time = true;
    public float first_orientation;  // Orientacion inicial del gyroscopio.
    public float sensorDistance;          // Distancia medida entre el sharp y el lidar.
    public float rotation_robot;    // Rotacion del robot (Giroscopio).
    public float pointOrientation;   // Posicion del motor frontal. (Direccion del sensor de distancia).
    public GameObject sensorObject;
    public float sensorAngle;
    public float LDistance, RDistance, lAngle=-999, rAngle=-999; //Para la derecha hacia adelante es mayor angulo. Alreves en la izquierda
    public float wheelRadius, displacement;

    public Vector3 tpoint, actualPose, auxPose;
    public int maxDistance = 500;   // Maxima distancia leida por los sensores (Se usa para escalar).
    public string robotIP = "ws://192.168.1.116:9090";  // IP del robot.
    

    private bool newReading = false;

    private float RightDisplacement(float diff)
    {
        //Si pase de 360 a 0
        if (diff <= -300)
        {
            return 360 + diff;
        }else if (diff >= 300)//Si pase de 0 a 360
        {
            return -(360 - diff);
        }

        return diff;
    }
    private float LeftDisplacement(float diff)
    {
        //Si pase de 360 a 0
        if (diff <= -300)
        {
            return -(360 + diff);
        }
        else if (diff >= 300)//Si pase de 0 a 360
        {
            return 360 - diff;
        }
        return -diff;
    }

    private void ReadArduino(string values)
    {
        string[] tokens = values.Split(',');
        float laux, raux, rdiff, ldiff, rdeltaW, ldeltaW;
        newReading = true;
        sensorDistance = int.Parse(tokens[1]);
        rotation_robot = float.Parse(tokens[0]);
        sensorAngle = int.Parse(tokens[2]);
        pointOrientation = (memesCalientes + sensorAngle -90);
        raux = float.Parse(tokens[4]);
        laux = float.Parse(tokens[3]);
        if (lAngle == -999)
        {
            lAngle = laux;
            rAngle = raux;
        }
        else
        {
            ldiff = laux - lAngle;
            rdiff = raux - rAngle;
            ldeltaW = RightDisplacement(rdiff);
            rdeltaW = LeftDisplacement(ldiff);
            RDistance = RightDisplacement(rdiff) * Mathf.Deg2Rad * wheelRadius;
            LDistance = LeftDisplacement(ldiff) * Mathf.Deg2Rad * wheelRadius;
            displacement = RDistance + LDistance / 2;
            lAngle = laux;
            rAngle = raux;
        }
        //Debug.Log(tokens[0]+ " Dist: " + tokens[1] + " SensDir: " + tokens[2] + " RW: " + tokens[3] + " LW: " + tokens[4]);
    }

    private void SubscriptionHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        ReadArduino(standardString.data);
    }

    void Start () {
        memesCalientes = transform.rotation.eulerAngles.y;
        auxPose = transform.position;
        RosSocket rosSocket = new RosSocket(robotIP);
        int subscription_id = rosSocket.Subscribe("/arduino", "std_msgs/String", SubscriptionHandler);
    }

    void CreateCube()
    {   
        float scaled = (sensorDistance / maxDistance) * 10;
        Vector3 rotationVector = transform.rotation.eulerAngles;
        //rotationVector.y = rotation_robot;                      // Asignar rotacion del gyro.
        //transform.rotation = Quaternion.Euler(rotationVector);
        //transform.position = new Vector3(scaled, 0f, transform.position.z);
        tpoint = new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointOrientation), 0, Mathf.Cos(Mathf.Deg2Rad * pointOrientation)) * scaled;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //cube.AddComponent<Rigidbody>();
        cube.transform.position = transform.position + tpoint;
        if (sensorDistance <= 35)
        {
            (cube.GetComponent<Renderer>()).material.color = new Color(0.5f, 1, 1);
        }
        Destroy(cube, 120.0f);
    }

    void Update()
    {
        memesCalientes = transform.rotation.eulerAngles.y;
    }

    void FixedUpdate()
    {
        if (newReading)
        {
            CreateCube();
            sensorObject.transform.rotation = Quaternion.AngleAxis(-sensorAngle, new Vector3(0, 1, 0));
            auxPose += transform.forward * displacement;
            
            //transform.position += transform.forward * displacement * 0.01f;
            newReading = false;
        }
    }
}
