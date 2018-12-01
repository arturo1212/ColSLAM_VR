using UnityEngine.AI;
using UnityEngine;

public class Explore : State
{
    Movement mov;
    NaiveMapping naiv;
    int nscans = 0, auxScan = -1;

    bool faced = false;
    public Explore(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
    }

    public override void Circunloquio()
    {
        Debug.Log("Init Explore");
        mov.proximatePoint = mov.metaPoint;
        nscans = 0;
        auxScan = -1;
        faced = false;
        mov.pathObstructed = false;
        mov.behaviourIsRunning = true;
        mov.traceDone = false;
        mov.facing = false;
    }

    public override void Colofon()
    {
        // TODO esto es mientras no se escoge un punto aleatorio
        if (!mov.pathObstructed)
        {
            //mov.metaPoint = null;
        }
        Debug.Log("RIP trace");
        mov.facing = false;
        mov.traceDone = false;
        mov.behaviourIsRunning = false;
        mov.prision = false;
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
        float angleThresh = 20f, radius=0.25f;

        if (!faced)
        {
            SteeringBehaviours.Face(mov, mov.proximatePoint, angleThresh, radius, true);
            faced = !mov.facing;
        }

        if (faced)
        {
            Debug.Log("Escaneando");
            updateNScans();
        }

        if (nscans >= 1 && faced)
        {

            if ((mov.proximatePoint - owner.transform.position).magnitude > naiv.maxDistance/naiv.scale)
            {
                mov.proximatePoint = Vector3.MoveTowards(owner.transform.position, mov.proximatePoint, (naiv.maxDistance / naiv.scale)*0.75f );
            }
            Debug.Log("Collision Raycasting");
            // Si hay algo en el medio RIP
            Debug.DrawRay(owner.transform.position, (mov.proximatePoint - owner.transform.position), Color.green, 30);
            Vector3 directionVector = (mov.proximatePoint - owner.transform.position).normalized;
            float distance = (owner.transform.position - mov.proximatePoint).magnitude;
            bool ray1 = Physics.Raycast(owner.transform.position, directionVector, distance, LayerMask.GetMask("Ghobstacles"));
            if (mov.debug)
            {
                Debug.DrawRay(owner.transform.position, directionVector * distance, Color.yellow, 10);
            }

            if (ray1)
            {
                Debug.Log("Path Obstructed");
                if (mov.greenPoint != null)
                {
                    Vector3 greenAux = (Vector3)mov.greenPoint;
                    mov.greenPoint = null;
                    mov.calculateMetaPoint();
                    mov.greenPoint = greenAux;
                    mov.pathObstructed = true;
                    return;
                }
                mov.calculateMetaPoint();
                mov.pathObstructed = true;
            }
            else
            {
                Debug.Log("All clear");
                mov.proximatePoint = mov.metaPoint;
                mov.traceDone = true;
            }

        }
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
