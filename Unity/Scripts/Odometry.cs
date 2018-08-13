using System.Collections.Generic;
using UnityEngine;

public class Odometry : MonoBehaviour {
    //posicion anterior, posicion actual.
    // Promedio anterior y actual.
    float prev_time;
    Vector3 pos_pre;
    Vector3 avg_pre = new Vector3(0f, 0f, 0f);
    List<Vector3> pos_list = new List<Vector3>();
    private PorfavorFunciona rosComm;

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
        float alpha1, alpha2, alpha3, alpha4;   // Factores en las probabilidades.

        alpha1 = alpha2 = alpha3 = alpha4 = 1;
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
	
	// Update is called once per frame
	void Update ()
    { // Si ventana de tiempo, entonces:
        if (Time.realtimeSinceStartup - prev_time > 2f)
        {
            Vector3 x_p = position_avg(pos_list);           // Calcular position_avg actual.
            Vector3 u = update_model(pos_pre, x_p);         // Update model (u)
            Vector3 x_n = odometry_sampling(pos_pre, u);    // Odometry sampling
            
            //Actualizar transform y rotacion
            transform.position = new Vector3(x_n.x, transform.position.y, x_n.y);
            Vector3 rotationVector = transform.rotation.eulerAngles;
            rotationVector.y = x_n.z;                      // Asignar rotacion.
            transform.rotation = Quaternion.Euler(rotationVector);

            pos_pre = x_n;                                  // Actualizar pre.
            pos_list.Clear();                               // Limpiar lista y agregar comienzo.
            pos_list.Add(x_n);
            prev_time = Time.realtimeSinceStartup;
        }
        else
        {
            //Procesar lecturas de ruedas aqui
            pos_list.Add(new Vector3(rosComm.auxPose.x, rosComm.auxPose.z, rosComm.rotation_robot * Mathf.Deg2Rad));
        }
        // No hacer nada, seguir llenando lista.
    }
}
