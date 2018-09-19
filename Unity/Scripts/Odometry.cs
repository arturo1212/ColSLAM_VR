using System.Collections.Generic;
using UnityEngine;

public class Odometry : MonoBehaviour
{
    //posicion anterior, posicion actual.
    // Promedio anterior y actual.
    public float alpha1 = 0.7f, alpha2 = 0.8f;   // Factores en las probabilidades.
    float prev_time;
    Vector3 pos_pre;
    Vector3 avg_pre = new Vector3(0f, 0f, 0f);
    List<Vector3> pos_list = new List<Vector3>();
    private NaiveMapping rosComm;

    // Cotas
    public float threshold=0.2f, rotThreshold=0.2f;

    // Variables de movimiento
    public float gyro_reading=0, prev_gyro_reading;       // Utilizados en isRotating()
    public float prevRotation, diffRot;                 // Correcion de la rotacion
    public bool isQuiet = false, wasQuiet = false;
    bool running = true;

    #region modelo prob
    public Vector3 position_avg(List<Vector3> positions)
    {   // Devuelve el promedio en cada coordenada.
        Vector3 result = new Vector3();
        foreach (Vector3 position in positions)
        {
            result = result + position;
        }
        return result / positions.Count;
    }

    public Vector3 update_model(Vector3 avg_prev, Vector3 avg_act)
    {   // Devuelve la variacion (PROMEDIO) en las rotaciones y la velocidad.
        float rota1 = Mathf.Atan2(avg_act.y - avg_prev.y, avg_act.x - avg_prev.x) - avg_prev.z;
        float rota2 = avg_act.z - avg_prev.z - rota1;
        float trans = Mathf.Sqrt(Mathf.Pow(avg_act.y - avg_prev.y, 2) + Mathf.Pow(avg_act.x - avg_prev.x, 2));
        return new Vector3(rota1, rota2, trans);
    }

    public float sample_normal(float b)
    {   // Distribucion normal.
        float aux = 0;
        for (int i = 0; i < 12; i++)
        {
            aux += Random.Range(-b, b);
        }
        return 1 / 2 * aux;
    }

    public Vector3 odometry_sampling(Vector3 pos_prev, Vector3 u)
    {
        //OJO comparr con las laminas
        float rota1 = u.x + sample_normal(alpha1 * u.x * u.x + (1 - alpha1) * u.z * u.z);
        float rota2 = u.y + sample_normal(alpha1 * u.y * u.y + (1 - alpha1) * u.z * u.z);
        float trans = u.z + sample_normal(alpha2 * u.z + (1 - alpha2) * (u.x * u.x + u.y * u.y));

        float x_new = pos_prev.x + trans * Mathf.Cos(pos_prev.z + rota1);
        float y_new = pos_prev.y + trans * Mathf.Sin(pos_prev.z + rota1);
        float w_new = pos_prev.z + rota1 + rota2;
        return new Vector3(x_new, y_new, w_new);
    }
    #endregion

    #region mov helpers
    bool isShaking(float gyro_reading, float thresh)
    {
        float diff = prev_gyro_reading - gyro_reading;

        if (diff <= -300)
        {
            diff = 360 + diff;
        }
        else if (diff >= 300)//Si estoy de 360 a 0
        {
            diff = -(360 - diff);
        }
        if (Mathf.Abs(diff) >= thresh)
            print("Sendo espasmo: " + diff.ToString() + "    " + prev_gyro_reading.ToString() + "    " + gyro_reading.ToString());
        return Mathf.Abs(diff) >= thresh;
    }

    bool IsDisplacing()
    {
        if (rosComm.displacement >= threshold)
            print("Desplazando!");
        return rosComm.displacement >= threshold;
    }

    bool isRotating()
    {
        if (Mathf.Abs(prev_gyro_reading - gyro_reading) > rotThreshold)
            print("Rotando!");
        return Mathf.Abs(prev_gyro_reading - gyro_reading) > rotThreshold;
    }
    #endregion

    // Use this for initialization
    void Start()
    {
        prev_time = Time.realtimeSinceStartup;
        rosComm = GetComponent<NaiveMapping>();
        pos_pre = new Vector3(rosComm.auxPose.x, rosComm.auxPose.z, transform.rotation.y); //new Vector3(transform.position.x, transform.position.z, transform.rotation.y);
        //Task stop_monitor = new Task(() => stop_detection());
        //Task quiet_task = new Task(() => quiet_handler());
        //stop_monitor.Start();
        //quiet_task.Start();
    }

    void OnApplicationQuit()
    {
        running = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        gyro_reading = rosComm.rotation_robot;    // fijar primera lectura.
        if (Time.realtimeSinceStartup - prev_time > 1f)
        {
            foreach (Vector3 lectura in pos_list)
            {
                // Funciones del modelo.
                Vector3 x_p = lectura;                          // Calcular position_avg actual.
                Vector3 u = update_model(pos_pre, x_p);         // Update model (u)
                Vector3 x_n = odometry_sampling(pos_pre, u);    // Odometry sampling

                //Actualizar transform y rotacion
                transform.position = new Vector3(x_n.x, transform.position.y, x_n.y) * (0.85f / rosComm.maxDistance);
                Vector3 rotationVector = transform.rotation.eulerAngles;
                rotationVector.y = AngleHelpers.angleToPositive(x_n.z) * Mathf.Rad2Deg;     // Asignar rotacion.
                transform.rotation = Quaternion.Euler(rotationVector);
                pos_pre = x_n;                              // Actualizar pre.
            }
            pos_list.Clear();                               // Limpiar lista y agregar comienzo.
            prev_time = Time.realtimeSinceStartup;
            prev_gyro_reading = gyro_reading;
        }
        else
        {
            float currentRotation = AngleHelpers.angleToPositive(gyro_reading + diffRot);
            pos_list.Add(new Vector3(rosComm.auxPose.x, rosComm.auxPose.z, currentRotation * Mathf.Deg2Rad));
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.y = rosComm.rotation_robot;
            transform.rotation = Quaternion.Euler(rotationVector);
            //pos_list.Add(new Vector3(0f, 0f, 0f));

        }
    }

    void stop_detection()
    {
        while (running)
        {
            bool cond1 = isRotating();
            bool cond2 = IsDisplacing();
            if (!cond1 && !cond2)
            {
                print("Entro stop");
                isQuiet = true;
            }
            else
            {
                print("salgo stop" + cond1.ToString() + "   " + cond2.ToString());
                isQuiet = false;
            }
        }
    }

    void quiet_handler()
    {
        while (running)
        {
            float gyro_angle = rosComm.rotation_robot;
            if (isQuiet && !wasQuiet)
            {
                print("QUIET HANDLER ENTRANDO");
                prevRotation = gyro_angle; // Guardar rotacion
                wasQuiet = true;
            }
            else if (!isQuiet && wasQuiet && !isShaking(gyro_angle, 50f))
            {
                diffRot += AngleHelpers.angleDifference(prevRotation, gyro_angle); // Calcular diff
                diffRot = AngleHelpers.angleToPositive(diffRot);
                wasQuiet = false;
            }
        }
    }
}
