using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Cleaning : State
{
    Movement mov;
    NaiveMapping naiv;
    Vector3 cleanPoint;
 
    float initialDist;
    public Cleaning(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();

    }

    public override void Circunloquio()
    {
        int i = Random.Range(0, mov.metaPoints.Count);
        cleanPoint = mov.metaPoints[i].transform.position;
        //Debug.Log("Punto de limpieza: "+cleanPoint);
        mov.behaviourIsRunning = true;
        mov.facing = true;
    }

    public override void Colofon()
    {
        mov.behaviourIsRunning = false;
        mov.facing = false;
        mov.prision = false;
    }

    public override void Execute()
    {

        float angleThresh = 20f;
       
        //Debug.Log(faced);
        if (mov.facing)
        {
            Debug.Log("Clean Facing");
            SteeringBehaviours.Face(mov, cleanPoint, angleThresh, true);
        }
        else
        {
            Debug.Log("Cleaned");
            mov.Stop(true);
        }
        
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
