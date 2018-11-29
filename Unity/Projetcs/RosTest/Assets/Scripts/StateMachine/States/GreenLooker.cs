using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Greenlooker : State
{
    Movement mov;
    NaiveMapping naiv;

    bool faced = false;
    float initialDist;
    int nscans = 0, auxScan = -1;

    public Greenlooker(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
    }

    void updateNScans()
    {
        if (naiv.sensorAngle == 90)
        {
            auxScan += 1;
            auxScan = auxScan % 3;
        }
        if (auxScan == 2)
        {
            nscans++;
            auxScan = -1;
        }
    }

    public override void Circunloquio()
    {
        mov.facing = true;
        nscans = 0;
        auxScan = -1;
        mov.behaviourIsRunning = true;
    }

    public override void Colofon()
    {

        mov.behaviourIsRunning = false;
        mov.facing = false;
    }

    public override void Execute()
    {
        float angleThresh = 20;
        //Debug.Log(faced);
        if (mov.facing)
        {
            Debug.Log("Facing holdCube");
            SteeringBehaviours.Face(mov, naiv.holdCube.transform.position, angleThresh, 0.2f, true);
        }

        if (!mov.facing)
        {
            updateNScans();
        }

        /* Esperar a estar detenido y acumular N iteraciones */
        if (nscans >= 1 && !mov.facing)
        {
            if (naiv.holdCube.name != "marker")
            {
                //Calcular nuevo greepnoint
                float randomAngle = Random.Range(0, 360);
                float circunferenceRadius = 0.3f;
                Vector3 nuevopunto = new Vector3(Mathf.Sin(randomAngle), 0, Mathf.Cos(randomAngle)) * circunferenceRadius;
                nuevopunto += naiv.holdCube.transform.position;

                int timesGenerated = 0;
                while ( timesGenerated<500 && Physics.Raycast(owner.transform.position, (nuevopunto - owner.transform.position).normalized, (owner.transform.position - nuevopunto).magnitude, LayerMask.GetMask("Obstacles")))
                {
                    randomAngle = Random.Range(0, 360);
                    nuevopunto = new Vector3(Mathf.Sin(randomAngle), 0, Mathf.Cos(randomAngle)) * circunferenceRadius;
                    nuevopunto += naiv.holdCube.transform.position;
                    timesGenerated++;
                }
                mov.greenPoint = nuevopunto;
                mov.calculateMetaPoint();
                //GameObject gr = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                //gr.transform.position = nuevopunto;
                //gr.transform.localScale = new Vector3(0.1f, 1, 0.1f);

            }
            mov.Stop(true);
        }
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
