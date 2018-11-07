﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class TracingExplore : State
{
    Movement mov;
    NavMeshPath path;
    NaiveMapping naiv;
    int nscans = 0, scanid = -1;
    Vector3 destiny = new Vector3(-1, -1, -1);
    bool faced = false;
    float initialDist;
    public TracingExplore(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
        path = mov.explorePath;
    }

    private void DrawPath()
    {
        Vector3 prev = path.corners[0];
        for(int i = 1; i < path.corners.Length; i++)
        {
            Debug.DrawRay(prev, path.corners[i]-prev, Color.blue, 30);
            prev = path.corners[i];
        }
    }

    public override void Circunloquio()
    {
        Debug.Log("Init Explore");
        path.ClearCorners();
        NavMesh.CalculatePath(owner.transform.position, (Vector3)mov.clickedPoint, NavMesh.AllAreas, path);
        
        mov.proximatePoint = new Vector3(-1, -1, -1);
        nscans = 0;
        faced = false;
        mov.behaviourIsRunning = true;
        mov.traceDone = false;
        mov.pathObstructed = false;
        mov.facing = false;
    }

    public override void Colofon()
    {
        // TODO esto es mientras no se escoge un punto aleatorio
        if (!mov.pathObstructed)
        {
            mov.clickedPoint = null;
        }
        Debug.Log("RIP trace");
        mov.facing = false;
        mov.traceDone = false;
        mov.behaviourIsRunning = false;
    }

    private Vector3 nextPoint(float radius)
    {
        Vector3 arrived = new Vector3(-1, -1, -1);
        float color1 = Random.value, color2 = Random.value, color3 = Random.value;
        foreach (Vector3 point in path.corners)
        {
            
            if ((point - mov.transform.position).magnitude > radius)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.GetComponent<SphereCollider>().enabled = false;
                sphere.transform.position = point;
                sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                (sphere.GetComponent<Renderer>()).material.color = new Color(color1, color2, color3);
                return point;
            }
        }
        Debug.Log("NO HAY PUNTO PROXIMO");
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
        //Debug.Log(faced);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.Log("Calculating path...");
            return;
        }
        DrawPath();
        float radius = 0.2f, angleThresh = 20;
        if (mov.proximatePoint.y == -1)
        {
            mov.proximatePoint = nextPoint(radius);
            if (mov.proximatePoint.y == -1) // TODO no hay punto proximo (Que hacer ? )
            {
                mov.Stop(true);
                mov.behaviourIsRunning = false;
                nscans = 0;
                return;
            }
            initialDist = (mov.proximatePoint - owner.transform.position).magnitude;
        }

        //Debug.Log(faced);
        if (!faced)
        {
            Debug.Log("Facing proximate point: " + mov.proximatePoint);
            SteeringBehaviours.Face(mov, mov.proximatePoint, angleThresh, true);
            faced = !mov.facing;
        }

        if (faced)
        {
            updateNScans();
        }

        /* Esperar a estar detenido y acumular N iteraciones */
        if (nscans >= 3 && faced)
        {

            Debug.Log("Now Going");
            RaycastHit hit;
            // Si hay algo en el medio RIP
            Debug.DrawRay(owner.transform.position, (mov.proximatePoint - owner.transform.position), Color.green,30);
            if (Physics.Raycast(owner.transform.position, (mov.proximatePoint - owner.transform.position).normalized, out hit, (owner.transform.position - mov.proximatePoint).magnitude)) 
            {
                Debug.Log("Path Obstructed");
                mov.pathObstructed = true;
            }
            else
            {
                Debug.Log("All clear");
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.GetComponent<SphereCollider>().enabled = false;
                sphere.transform.position = mov.proximatePoint;
                sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                (sphere.GetComponent<Renderer>()).material.color = new Color(1, 0, 0);
                mov.traceDone = true;
            }
            
        }
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
