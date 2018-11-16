using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Greenlooker : State
{
    Movement mov;
    NavMeshPath path;
    NaiveMapping naiv;
    int nscans = 0, auxScan = -1;
    Vector3 destiny = new Vector3(-1, -1, -1);
    NavMeshSurface surface;

    bool faced = false;
    float initialDist;
    public Greenlooker(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
        path = mov.explorePath;
    }


    public override void Circunloquio()
    {
    }

    public override void Colofon()
    {

        mov.behaviourIsRunning = false;
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

    public override void Execute()
    {
        float angleThresh = 20;
        //Debug.Log(faced);
        if (!faced)
        {
            Debug.Log("Facing proximate point: " + mov.proximatePoint);
            SteeringBehaviours.Face(mov, naiv.holdCube.transform.position, angleThresh, true);
            faced = !mov.facing;
        }

        if (faced)
        {
            updateNScans();
        }

        /* Esperar a estar detenido y acumular N iteraciones */
        if (nscans >= 1 && faced)
        {
            if (naiv.holdCube.name != "marker")
            {
                //Calcular nuevo greepnoint
                Vector3 nuevopunto = (Vector3)mov.greenPoint + Random.insideUnitSphere * 0.2f;
                nuevopunto.y = 0;
                mov.greenPoint = nuevopunto;
                mov.Stop(true);
            }

        }
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
