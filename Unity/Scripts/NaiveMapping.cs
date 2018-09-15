using UnityEngine;
using RosSharp.RosBridgeClient;


public class NaiveMapping : MonoBehaviour {
    public float memesCalientes;
    public bool firstTime = false;
    public float first_orientation;  // Orientacion inicial del gyroscopio.
    public float sensorDistance;          // Distancia medida entre el sharp y el lidar.
    public float rotation_robot;    // Rotacion del robot (Giroscopio).
    public float pointOrientation;   // Posicion del motor frontal. (Direccion del sensor de distancia).
    public GameObject sensorObject;
    public float sensorAngle;
    public float LDistance, RDistance, lAngle=0, rAngle=0; //Para la derecha hacia adelante es mayor angulo. Alreves en la izquierda

    public float angle_thresh = 0.2f;
    public float wheelRadius, displacement;

    public Vector3 tpoint, actualPose, auxPose;
    public int maxDistance = 100;   // Maxima distancia leida por los sensores (Se usa para escalar).
    public string robotIP = "ws://192.168.1.105:9090";  // IP del robot.
    private bool newReading = false;

    private void ReadArduino(string values)
    {
        // Parsear argumentos
        float laux, raux, rdeltaW = 0, ldeltaW = 0;
        string[] tokens = values.Split(',');
        newReading = true;  // Para que?

        // Modulo de Distancia (con angulo de motor)
        sensorDistance = int.Parse(tokens[1]);
        sensorAngle = int.Parse(tokens[2]);                         // ANGULO (del motor)
        pointOrientation = (memesCalientes + sensorAngle - 90);     // ANGULO (Verificar rangos)

        // Movimiento del robot
        rotation_robot = AngleHelpers.Among360(float.Parse(tokens[0]));  // ANGULO (Gyroscopio)
        raux = float.Parse(tokens[4]);  // Angulo (Rueda derecha)
        laux = float.Parse(tokens[3]);  // Angulo (Rueda izquierda)

        /* Si no es la primera vez, calcular cosas */
        if(!firstTime)
        {
            print("First time: " + lAngle.ToString() + "   " + rAngle.ToString());
            lAngle = laux;
            rAngle = raux;
            firstTime = true;
            return;
        }
        if(Mathf.Abs(lAngle - laux) < angle_thresh || Mathf.Abs(rAngle - raux) < angle_thresh)
        {
            displacement = 0f;
            return;
        }
        ldeltaW = AngleHelpers.angleDifference(lAngle, laux);
        rdeltaW = AngleHelpers.angleDifference(rAngle, raux);
        RDistance = - rdeltaW * Mathf.Deg2Rad * wheelRadius;
        LDistance = ldeltaW * Mathf.Deg2Rad * wheelRadius;
        displacement = Mathf.Abs(RDistance - LDistance) >= Mathf.Abs(RDistance + LDistance) ? 0 : RDistance + LDistance / 2;    // Formula para no moverse rotando.
        print("Distancias: " + RDistance.ToString() + "  " + LDistance.ToString() + "  " + displacement.ToString());
        lAngle = laux;
        rAngle = raux;
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
        float scaled = (sensorDistance / maxDistance);
        Vector3 rotationVector = transform.rotation.eulerAngles;
        tpoint = new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointOrientation), 0, Mathf.Cos(Mathf.Deg2Rad * pointOrientation)) * scaled;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
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
        //Vector3 rotationVector = transform.rotation.eulerAngles;
        //rotationVector.y = rotation_robot;
        //transform.rotation = Quaternion.Euler(rotationVector);
    }

    void FixedUpdate()
    {
        if (newReading)
        {
            CreateCube();
            sensorObject.transform.rotation = Quaternion.AngleAxis(-sensorAngle, new Vector3(0, 1, 0));
            auxPose += transform.forward * displacement;
            newReading = false;
        }
    }
}
