using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class ExploreMove : State
{
    Movement mov;
    float initialDist;
    public ExploreMove(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
    }

    public override void Circunloquio()
    {
        Debug.Log("Quiero ir al goal");
        mov.behaviourIsRunning = true;
        mov.arrivedGreen = false;
    }

    public override void Colofon()
    {
        mov.behaviourIsRunning = false;
        mov.arrivedGreen = false;
        if (mov.greenPoint == null)
        {
            mov.counter++;
        }
    }

    public override void Execute()
    {
        float radius = mov.greenPoint != null ? 0.2f : 0.2f;
        //Debug.Log("Now Going");
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = mov.proximatePoint;
        sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        (sphere.GetComponent<Renderer>()).material.color = new Color(1, 0, 0);
        SteeringBehaviours.GoToGoal(mov, mov.proximatePoint, radius, 25, true);
 
        if ((mov.transform.position - mov.proximatePoint).magnitude < radius)
        {
            Debug.Log(mov.greenPoint != null ? "Listo ExploreMove GREEN" : "Listo ExploreMove State");
            mov.behaviourIsRunning = false;
            if (mov.greenPoint == null)
            {
                mov.Stop(true);
            }
            else
            {
                mov.arrivedGreen = true;
            }
        }

    }

    public override string ToString()
    {
        return base.ToString();
    }
}
