using RosSharp.RosBridgeClient;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

public class NaiveMapping : MonoBehaviour
{
    public bool firstTime = false;
    public float first_orientation;  // Orientacion inicial del gyroscopio.
    public float sensorDistance;          // Distancia medida entre el sharp y el lidar.
    public float rotation_robot;    // Rotacion del robot (Giroscopio).
    public float pointOrientation;   // Posicion del motor frontal. (Direccion del sensor de distancia).
    public GameObject sensorObject, obstaclePrefab;
    public float sensorAngle;
    public float LDistance, RDistance, lAngle = 0, rAngle = 0; //Para la derecha hacia adelante es mayor angulo. Alreves en la izquierda


    public float angle_thresh = 0.2f;
    public float wheelRadius, displacement, minDistance, maxDistance;

    public int scale = 100, scanNumber=0, holdCounter=0;   // Maxima distancia leida por los sensores (Se usa para escalar).
    private bool newReading = false;

    [HideInInspector]
    public int calibrtionSubcription_id, arduinoSubscription_id = -1, markerSubcription_id, auxScan = -1, objectiveSubscription_id = -1, streamSubscription_id = -1;
    [HideInInspector]
    public Vector3 tpoint, actualPose, auxPose;
    [HideInInspector]
    Robot robot;
    [HideInInspector]
    Vector3 savedPosition;
    [HideInInspector]
    public GameObject lastCube;
    public GameObject holdCube=null, redCube = null;
    [HideInInspector]
    public string visionstate="", visionobj = "";
    float y1, y2;
    public bool markerFound = false;
    public byte[] imageBytes;


    private void ReadArduino(string values)
    {
        // Parsear argumentos
        float laux, raux, rdeltaW = 0, ldeltaW = 0;
        string[] tokens = values.Split(',');
        newReading = true;  // Para que?

        // Modulo de Distancia (con angulo de motor)
        sensorDistance = float.Parse(tokens[1]);
        sensorAngle = float.Parse(tokens[2]);                         // ANGULO (del motor)
        pointOrientation = (rotation_robot + sensorAngle - 90);     // ANGULO (Verificar rangos)

        // Movimiento del robot
        rotation_robot = AngleHelpers.angleToPositive(float.Parse(tokens[0]));  // ANGULO (Gyroscopio)
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
        sensorDistance = float.Parse(tokens[1]);
        sensorAngle = float.Parse(tokens[2]);                         // ANGULO (del motor)
        pointOrientation = (rotation_robot + sensorAngle - 90);     // ANGULO (Verificar rangos)

        // Movimiento del robot
        rotation_robot = AngleHelpers.angleToPositive(float.Parse(tokens[0]));  // ANGULO (Gyroscopio)
        raux = float.Parse(tokens[4]);  // Angulo (Rueda derecha)
        laux = float.Parse(tokens[3]);  // Angulo (Rueda izquierda)

        /* Si no es la primera vez, calcular cosas */
        //print("First time: " + lAngle.ToString() + "   " + rAngle.ToString());
        lAngle = laux;
        rAngle = raux;
        robot.rosSocket.Unsubscribe(calibrtionSubcription_id);
        arduinoSubscription_id = robot.rosSocket.Subscribe("/arduino", "std_msgs/String", SubscriptionHandler);
        StandardString msg = new StandardString
        {
            data = "reset"
        };
        robot.rosSocket.Publish(robot.resetPublisherId, msg);
    }

    private void MarkerReader(string message)
    {
        visionstate = message;
    }

    private void ObjectiveReader(string message)
    {
        visionobj = message;
    }

    private void StreamReader(string message)
    {
        imageBytes = Convert.FromBase64String(message);
    }

    private void SubscriptionStreamHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        StreamReader(standardString.data);
    }

    private void SubscriptionMarkHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        MarkerReader(standardString.data);
    }

    private void SubscriptionObjectiveHandler(Message message)
    {
        StandardString standardString = (StandardString)message;
        ObjectiveReader(standardString.data);
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
        auxPose = transform.position;
        robot = gameObject.GetComponent<Robot>();
        if (robot.rosSocket == null)
        {
            Debug.Log("ROSSOCKET ES NULL");
        }
        calibrtionSubcription_id = robot.rosSocket.Subscribe("/arduino", "std_msgs/String", CalibrationSubscritpionHandler);
        markerSubcription_id = robot.rosSocket.Subscribe("/homography", "std_msgs/String", SubscriptionMarkHandler);
        objectiveSubscription_id = robot.rosSocket.Subscribe("/objective", "std_msgs/String", SubscriptionObjectiveHandler);
        streamSubscription_id = robot.rosSocket.Subscribe("/stream", "std_msgs/String", SubscriptionStreamHandler);
    }

    void CreateTag(string s)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s)) { return; }
        }
        tagsProp.InsertArrayElementAtIndex(0);
        SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
        n.stringValue = s;
        tagManager.ApplyModifiedProperties();
    }

    void DestroyObstacleList(List<GameObject> l)
    {
        foreach(GameObject g in l)
        {
            //Debug.Log("Cleaned");
            //aqui deberia estar el trigger a reducir el contador; ObtsalceLost
            GetComponentInParent<GridGenerator>().ObstacleLost(g.transform.position);
            Destroy(g);
        }
    }

    void CreateCube()
    {
        /* Vector Centro -> LIDAR -> Obstaculo */
        var offset = new Vector3(Mathf.Sin(Mathf.Deg2Rad * rotation_robot), 0, Mathf.Cos(Mathf.Deg2Rad * rotation_robot)) * 8.5f / scale;               // Separacion entre Centro y LIDAR
        tpoint = new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointOrientation), 0, Mathf.Cos(Mathf.Deg2Rad * pointOrientation)) * sensorDistance / scale;   // Punto de obstaculo

        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position + offset, tpoint.normalized, (sensorDistance + 20) / scale, LayerMask.GetMask("Obstacles"));

        if (hits.Length > 0)
        {

            RaycastHit hit;
            List<GameObject> todelete = new List<GameObject>();
            for(int i = 0; i<hits.Length; i++)
            {
                hit = hits[i];
                if ( hit.transform.gameObject.name != gameObject.name + scanNumber && hit.transform.gameObject.name != "keep" && hit.transform.gameObject.name != "marker")
                {
                    todelete.Add(hit.transform.gameObject);
                }
            }
            DestroyObstacleList(todelete);
        }

        //RaycastHit hit;
        //if (sensorDistance > 19 && Physics.Raycast(transform.position + offset, tpoint.normalized, out hit, (sensorDistance + 10) / scale, -1)) 
        //{
        //    Destroy(hit.transform.gameObject);
        //    hit.
        //}

        /* Crear obstaculo en la interfaz de Unity*/
        string tag = gameObject.name + scanNumber;
        //CreateTag(tag);
        lastCube = Instantiate(obstaclePrefab, transform.position + offset + tpoint, Quaternion.LookRotation(-tpoint.normalized, Vector3.up));
        lastCube.name = tag;
        lastCube.transform.parent = transform.parent;
        GetComponentInParent<GridGenerator>().ObstacleFound(transform.position + offset + tpoint);
        //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);      // Creaci�n de cubo b�sico (CAMBIAR POR PREFAB)
        //cube.transform.localScale = new Vector3(0.01f, 0.5f, 0.01f);      // Escala del cubito
        //cube.transform.position = transform.position + offset + tpoint; // Desplazamiento + punto = nuevo_punto
        //cube.AddComponent<BoxCollider>();
        //cube.AddComponent<NavMeshObstacle>().carving = true;
        //cube.name = tag;

        /* Diferenciar SHARP del LIDAR */
        //if (sensorDistance <= 35)
        //{
        //    (cube.GetComponent<Renderer>()).material.color = new Color(0.5f, 1, 1);
        //}

        /* Delay de destruccion */
        //Destroy(cube, 120.0f);
    }

    public void UpdateScanNumber()
    {
        /*if (scanAngle == 90)
        {
            auxScan += 1;
            auxScan = auxScan % 3;
        }
        if (auxScan == 2)
        {
            scanNumber += 1;
            auxScan = -1;
        }*/
        scanNumber++;

    }

    void Update()
    {
        if (newReading)
        {
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.y = rotation_robot;
            transform.rotation = Quaternion.Euler(rotationVector);
            // Crear cubos dentro del rango de vision y cubos verdes aunque esten lejos
            if (sensorDistance>minDistance && sensorDistance <= maxDistance || visionstate=="hold"  )
            {
                CreateCube();
            }
            else //Raycast de limpieza
            {

                var offset = new Vector3(Mathf.Sin(Mathf.Deg2Rad * rotation_robot), 0, Mathf.Cos(Mathf.Deg2Rad * rotation_robot)) * 8.5f / scale;

                RaycastHit[] hits;
                hits = Physics.RaycastAll(transform.position + offset, tpoint.normalized, (maxDistance + 20) / scale, LayerMask.GetMask("Obstacles"));
                List<GameObject> todelete = new List<GameObject>();
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.gameObject.name != gameObject.name + scanNumber && hit.transform.gameObject.name != "keep" && hit.transform.gameObject.name != "marker" && hit.transform.gameObject.name != "Rojito")
                    {
                        todelete.Add(hit.transform.gameObject);
                    }
                }
                DestroyObstacleList(todelete);
            }
            var rotation_sensor = transform.rotation.eulerAngles;
            rotation_sensor.y = sensorAngle;
            sensorObject.transform.rotation = Quaternion.Euler(rotation_sensor);
            transform.position = transform.position + transform.forward * 0.8f *displacement/scale;
            newReading = false;
        }

        // VISION
        if ( visionstate.Contains("hold") && !markerFound)// Guarda desde donde lo leiste
        {
            holdCounter++;
            Debug.Log("Vi VERDE");
            if(holdCube==null && lastCube == null)
            {
                return;
            }
            if (holdCube != null)
            {
                //Debug.Log("Destruyemeste");
                holdCube.name = "oldGreen";
            }
            //Debug.Log("DESPUES DEL IF");
            holdCube = lastCube != null ? lastCube : holdCube;
            holdCube.name = "keep";
            Vector3 newScale = holdCube.transform.localScale;
            newScale.y = 3;
            holdCube.transform.localScale = newScale;
            //Debug.Log("hold " + holdCube + " last " + lastCube);
            Movement mov = GetComponent<Movement>();

            // Generar punto cercano a la marca verde.
            var offset = new Vector3(Mathf.Sin(Mathf.Deg2Rad * rotation_robot), 0, Mathf.Cos(Mathf.Deg2Rad * rotation_robot)) * 8.5f / scale;               // Separacion entre Centro y LIDAR
            mov.greenPoint = new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointOrientation), 0, Mathf.Cos(Mathf.Deg2Rad * pointOrientation)) * (sensorDistance - 30) / scale;   // Punto de obstaculo
            mov.greenPoint += holdCube.transform.position;
            //GameObject gr = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            //gr.transform.position = (Vector3)mov.greenPoint;
            //gr.transform.localScale = new Vector3(0.1f, 1, 0.1f);
            //Debug.Log("Green Point seteado " + mov.greenPoint);
            visionstate = "";
        }
        else if (visionstate.Contains("found"))
        {
            Debug.Log("VI LA MARCA");
            string[] tokens = visionstate.Split(',');
            y1 = float.Parse(tokens[1]) * Mathf.Rad2Deg;
            y2 = float.Parse(tokens[2]) * Mathf.Rad2Deg;

            holdCube.name = "marker";
            holdCube.tag = "Marker";
            Debug.Log("Antes: " + holdCube.transform.rotation.eulerAngles);
            holdCube.transform.Rotate(Vector3.up, -y2);
            Debug.Log("Despues: " + holdCube.transform.rotation.eulerAngles);
            GetComponent<Movement>().greenPoint = null;

            //Detach del handler
            robot.rosSocket.Unsubscribe(markerSubcription_id);
            markerFound = true;
            visionstate = "";
        }
        if (!string.IsNullOrEmpty(visionobj))
        {
            if(lastCube!=null)
            {
                redCube = lastCube;
                redCube.name = "Rojito";
                redCube.tag = "Objectives";
                redCube.GetComponent<Renderer>().material.color = Color.red;
                visionobj = "";

            }
        }
    }


}
