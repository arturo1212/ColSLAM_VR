using UnityEngine;
using RosSharp.RosBridgeClient;


public class NaiveMapping : MonoBehaviour {
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

    public int scale = 100;   // Maxima distancia leida por los sensores (Se usa para escalar).
    public string robotIP = "ws://192.168.1.105:9090";  // IP del robot.
    private bool newReading = false;

    [HideInInspector]
    public int calibrtionSubcription_id, arduinoSubscription_id=-1;
    public Vector3 tpoint, actualPose, auxPose;
    public float memesCalientes;
    RosSocket rosSocket;
    
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

        if (Mathf.Abs(lAngle - laux) < angle_thresh || Mathf.Abs(rAngle - raux) < angle_thresh)
        {
            displacement = 0f;
            return;
        }
        ldeltaW = Mathf.DeltaAngle(laux, lAngle);
        rdeltaW = Mathf.DeltaAngle(raux, rAngle);
        RDistance = -rdeltaW * Mathf.Deg2Rad * wheelRadius;
        LDistance = ldeltaW * Mathf.Deg2Rad * wheelRadius;
        displacement = Mathf.Abs(RDistance - LDistance) >= Mathf.Abs(RDistance + LDistance) ? 0 : RDistance + LDistance / 2;    // Formula para no moverse rotando.
        //print("Distancias: " + RDistance.ToString() + "  " + LDistance.ToString() + "  " + displacement.ToString());
        lAngle = laux;
        rAngle = raux;
        //Debug.Log(tokens[0]+ " Dist: " + tokens[1] + " SensDir: " + tokens[2] + " RW: " + tokens[3] + " LW: " + tokens[4]);
    }

    private void Calibrate(string values)
    {
        // Parsear argumentos
        float laux, raux;
        string[] tokens = values.Split(',');

        // Modulo de Distancia (con angulo de motor)
        sensorDistance = int.Parse(tokens[1]);
        sensorAngle = int.Parse(tokens[2]);                         // ANGULO (del motor)
        pointOrientation = (memesCalientes + sensorAngle - 90);     // ANGULO (Verificar rangos)

        // Movimiento del robot
        rotation_robot = AngleHelpers.Among360(float.Parse(tokens[0]));  // ANGULO (Gyroscopio)
        raux = float.Parse(tokens[4]);  // Angulo (Rueda derecha)
        laux = float.Parse(tokens[3]);  // Angulo (Rueda izquierda)

        /* Si no es la primera vez, calcular cosas */
        print("First time: " + lAngle.ToString() + "   " + rAngle.ToString());
        lAngle = laux;
        rAngle = raux;
        rosSocket.Unsubscribe(calibrtionSubcription_id);
        arduinoSubscription_id = rosSocket.Subscribe("/arduino", "std_msgs/String", SubscriptionHandler);
    }

    private void SubscriptionHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        ReadArduino(standardString.data);
    }

    private void CalibrationSubscritpionHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        Calibrate(standardString.data);
    }

    void Start()
    {
        memesCalientes = transform.rotation.eulerAngles.y;
        auxPose = transform.position;
        rosSocket = new RosSocket(robotIP);
        calibrtionSubcription_id = rosSocket.Subscribe("/arduino", "std_msgs/String", CalibrationSubscritpionHandler);
    }


    void CreateCube()
    {   
        /* Vector Centro -> LIDAR -> Obstaculo */
        var offset = new Vector3(Mathf.Sin(Mathf.Deg2Rad * rotation_robot), 0, Mathf.Cos(Mathf.Deg2Rad * rotation_robot)) * 9.5f / scale;               // Separacion entre Centro y LIDAR
        tpoint = new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointOrientation), 0, Mathf.Cos(Mathf.Deg2Rad * pointOrientation)) * sensorDistance / scale;   // Punto de obstaculo

        /* DEBUG */
        var center_point = tpoint - offset;
        print(tpoint);

        /* Crear obstaculo en la interfaz de Unity*/
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);      // Creación de cubo básico (CAMBIAR POR PREFAB)
        cube.transform.localScale = new Vector3(0.02f, 1f, 0.02f);      // Escala del cubito
        cube.transform.position = transform.position + offset + tpoint; // Desplazamiento + punto = nuevo_punto

        /* Diferenciar SHARP del LIDAR */
        if (sensorDistance <= 35)
        {
            (cube.GetComponent<Renderer>()).material.color = new Color(0.5f, 1, 1);
        }

        /* Delay de destruccion */
        Destroy(cube, 120.0f);
    }

    void Update()
    {
        if (newReading)
        {
            memesCalientes = transform.rotation.eulerAngles.y;
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.y = rotation_robot;
            transform.rotation = Quaternion.Euler(rotationVector);
            CreateCube();
            var rotation_sensor = transform.rotation.eulerAngles;
            rotation_sensor.y = sensorAngle;
            sensorObject.transform.rotation = Quaternion.Euler(rotation_sensor);
            auxPose += transform.forward * displacement;
            newReading = false;
        }
    }


}
