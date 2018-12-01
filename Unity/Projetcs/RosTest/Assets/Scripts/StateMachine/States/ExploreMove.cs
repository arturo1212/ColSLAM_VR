using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using System.IO;

public class ExploreMove : State
{
    StreamWriter writer;
    Movement mov;
    float initialDist;
    Vector3 initialPosition;
    NaiveMapping naiv;
    string path;
    public ExploreMove(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
        string date = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt";
        path = "Assets/Resources/" + owner.transform.parent.name + '_' + date;
    }

    public override void Circunloquio()
    {
        Debug.Log("Quiero ir al goal");
        mov.behaviourIsRunning = true;
        mov.arrivedGreen = false;
        initialPosition = owner.transform.position;
    }

    public override void Colofon()
    {
        mov.behaviourIsRunning = false;
        mov.arrivedGreen = false;
        if (mov.greenPoint == null)
        {
            mov.counter++;
            mov.calculateMetaPoint();
        }
        float distance = (owner.transform.position - initialPosition).magnitude;
        writer = new StreamWriter(path, true);
        string line = distance.ToString() + "/" + (distance*naiv.scale).ToString();
        line += initialPosition + "/" + owner.transform.position;
        writer.WriteLine(line);
        writer.Close();
    }

    public override void Execute()
    {
        float radius = mov.greenPoint != null ? mov.greenArrive : 0.25f;
        //Debug.Log("Now Going");
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = mov.proximatePoint;
        sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        (sphere.GetComponent<Renderer>()).material.color = new Color(1, 0, 0);
        SteeringBehaviours.GoToGoal(mov, mov.proximatePoint, radius, 25, true);
 
        if ((mov.transform.position - mov.proximatePoint).magnitude < radius)
        {
            Debug.Log((mov.greenPoint != null ? "Listo ExploreMove GREEN " : "Listo ExploreMove State ")+owner.transform.parent.name);
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
