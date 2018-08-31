using System.Collections.Generic;
using UnityEngine;

public class Odometry : MonoBehaviour {
    //posicion anterior, posicion actual.
    // Promedio anterior y actual.
    public float alpha1, alpha2, alpha3, alpha4;   // Factores en las probabilidades.
    float prev_time;
    Vector3 pos_pre;
    Vector3 avg_pre = new Vector3(0f, 0f, 0f);
    List<Vector3> pos_list = new List<Vector3>();
    private PorfavorFunciona rosComm;
    public float threshold, rotThreshold, maxDisplacement=0;
    bool stillRotationSet = false, previous_firstTime = false, hadSpasm=false;
    public float prevRotation, currentRotation,diffRot, prev_gyro_reading;

    public Vector3 position_avg(List<Vector3> positions)
    {   // Devuelve el promedio en cada coordenada.
        Vector3 result = new Vector3();
        foreach(Vector3 position in positions)
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
        for (int i=0; i <12;i++){
            aux += Random.Range(-b, b);
        }
        return 1/2*aux;
    }

    public Vector3 odometry_sampling(Vector3 pos_prev, Vector3 u)
    {   
        //OJO comparr con las laminas
        float rota1 = u.x + sample_normal(alpha1 * u.x * u.x + alpha2 * u.z * u.z);
        float rota2 = u.y + sample_normal(alpha1 * u.y * u.y + alpha2 * u.z * u.z);
        float trans = u.z + sample_normal(alpha3 * u.z + alpha4 * (u.x*u.x + u.y*u.y));

        float x_new = pos_prev.x + trans * Mathf.Cos(pos_prev.z + rota1); 
        float y_new = pos_prev.y + trans * Mathf.Sin(pos_prev.z + rota1);
        float w_new = pos_prev.z + rota1 + rota2;
        return new Vector3(x_new, y_new, w_new);
    }

    // Use this for initialization
    void Start () {
        prev_time = Time.realtimeSinceStartup;
        rosComm = GetComponent<PorfavorFunciona>();
        pos_pre = new Vector3(0f, 0f, 0f);
    }
	 
    bool isShaking(float gyro_reading, float thresh)
    {
        float diff = currentRotation - gyro_reading;

        if (diff <= -300)
        {
            diff = 360 + diff;
        }
        else if (diff >= 300)//Si estoy de 360 a 0
        {
            diff = -(360 - diff);
        }
        if(Mathf.Abs(diff) >= thresh)
            print("Sendo espasmo: "+diff.ToString());
        return Mathf.Abs(diff) >= thresh;
    }

    bool isDisplacing()
    {
        if(rosComm.displacement >= threshold)
            print("Desplazando!");
        return rosComm.displacement >= threshold;
    }

    bool isRotating()
    {
        float rot = Mathf.Abs(rosComm.RDistance - rosComm.LDistance) - Mathf.Abs(rosComm.RDistance + rosComm.LDistance);
        if (rot >= rotThreshold)
            print("Rotando!");
        return rot >= rotThreshold;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        // Verificacion de primera lectura
        if (!rosComm.firstTime){
            return;
        }

        float gyro_reading = rosComm.rotation_robot;    // fijar primera lectura.

        if (!previous_firstTime)
        {
            prevRotation = gyro_reading;
            prev_gyro_reading = gyro_reading;
            previous_firstTime = true;
            return;
        }

        // Me quedo quieto
        if ( ( !isRotating() && !isDisplacing() && stillRotationSet == false ) || prevRotation==0 )
        {
            if (isShaking(gyro_reading, 10))
            {
                return;
            }
            prevRotation = gyro_reading;
            stillRotationSet = true;
        }
        //Comienzo a moverme
        else if (stillRotationSet == true && ( isRotating() || isDisplacing() ))
        {
            diffRot += prevRotation - gyro_reading;
            stillRotationSet = false;
        }
        if (!isDisplacing() && !isRotating())
        {
            currentRotation = prevRotation + diffRot;
        }
        else
        {
            if (isShaking(gyro_reading, 150) && !hadSpasm)
            {
                //hadSpasm = true;
                return;
            }
            Debug.Log("Believening in Gyro");
            currentRotation = gyro_reading + diffRot;
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.y = currentRotation;                      // Asignar rotacion.
            //transform.rotation = Quaternion.Euler(rotationVector);
            prevRotation = gyro_reading;
            hadSpasm = false;
        }

        prev_gyro_reading = gyro_reading;

        if (Time.realtimeSinceStartup - prev_time > 1f)
        {   
            //Debug.Log("Updating Model "+pos_list.Count);
            foreach (Vector3 lectura in pos_list)
            {
                Vector3 x_p = lectura;           // Calcular position_avg actual.
                Vector3 u = update_model(pos_pre, x_p);         // Update model (u)
                Vector3 x_n = odometry_sampling(pos_pre, u);    // Odometry sampling

                //Actualizar transform y rotacion
                //Debug.Log("New Pose " + x_n.ToString());
                transform.position = new Vector3(x_n.x, transform.position.y, x_n.y)*(0.85f / rosComm.maxDistance);
                Vector3 rotationVector = transform.rotation.eulerAngles;
                rotationVector.y = x_n.z * Mathf.Rad2Deg;                      // Asignar rotacion.
                //print(rotationVector.y);
                transform.rotation = Quaternion.Euler(rotationVector);
                //print(transform.rotation.eulerAngles.y);

                pos_pre = x_n;                                  // Actualizar pre.
            }
            pos_list.Clear();                               // Limpiar lista y agregar comienzo.
            prev_time = Time.realtimeSinceStartup;
        }
        else
        {
            //Procesar lecturas de ruedas aqui
            pos_list.Add(new Vector3(rosComm.auxPose.x, rosComm.auxPose.z, currentRotation * Mathf.Deg2Rad));
        }
        // No hacer nada, seguir llenando lista.
        
    }
}
