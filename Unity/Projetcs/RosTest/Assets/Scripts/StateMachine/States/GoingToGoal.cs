using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class GoingToGoal : State
{
    Movement mov;
    NavMeshPath path;
    NaiveMapping naiv;
    int nscans=0,scanid=-1;
    Vector3 destiny=new Vector3(-1,-1,-1);
    bool faced=false;
    float initialDist;
    public GoingToGoal(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
        path = new NavMeshPath();
    }

    public override void Circunloquio()
    {
        Debug.Log("Quiero ir al goal");
        nscans = 0;
        path.ClearCorners();
        NavMesh.CalculatePath(owner.transform.position,(Vector3)mov.metaPoint, NavMesh.AllAreas, path);
        faced = false;
        destiny = new Vector3(-1, -1, -1);
    }

    public override void Colofon()
    {
        mov.behaviourIsRunning = false;
    }

    private Vector3 nextPoint(float radius)
    {
        Vector3 arrived = new Vector3(-1, -1, -1);
        foreach (Vector3 point in path.corners)
        {
            if ((point - mov.transform.position).magnitude > radius)
            {
                return point;
            }
        }
        return arrived;
    }

    void updateNScans()
    {
        scanid = scanid == -1 ? naiv.scanNumber : scanid;
        if (scanid != naiv.scanNumber)
        {
            nscans++;
            scanid = naiv.scanNumber;
        }
    }

    public override void Execute()
    {
        float radius = 0.2f, angleThresh = 20;
        if (destiny.y == -1)
        {
            destiny = nextPoint(radius);
            if (destiny.y == -1)
            {
                mov.Stop(true);
                nscans = 0;
                return;
            }
            initialDist = (destiny - owner.transform.position).magnitude;
            mov.behaviourIsRunning = true;
        }

        //Debug.Log(faced);
        if (!faced)
        {
            Debug.Log("Facing");
            SteeringBehaviours.Face(mov, destiny, angleThresh, radius, true);
            faced = !mov.facing;
        }

        if (faced)
        {
            updateNScans();
        }

        if (nscans >= 2 && faced)
        {
            //Debug.Log("Now Going");
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = destiny;
            sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            (sphere.GetComponent<Renderer>()).material.color = new Color(1, 0, 0);
            SteeringBehaviours.GoToGoal(mov, destiny, radius, 25, true);
        }
        if ( (mov.transform.position - destiny).magnitude < radius)
        {
            Debug.Log("Litso GoToGoal State");
            mov.behaviourIsRunning = false;
            mov.Stop(true);
        }
        
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
