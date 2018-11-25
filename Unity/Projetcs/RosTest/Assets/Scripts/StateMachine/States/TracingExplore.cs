using UnityEngine.AI;
using UnityEngine;

public class TracingExplore : State
{
    Movement mov;
    NavMeshPath path;
    NaiveMapping naiv;
    int nscans = 0, auxScan=-1;
    Vector3 destiny = new Vector3(-1, -1, -1);
    NavMeshSurface surface;

    bool faced = false;
    float initialDist, initialTime;
    public TracingExplore(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
        naiv = owner.GetComponent<NaiveMapping>();
        path = mov.explorePath;
        if (path != null)
        {
            Debug.Log("Path accesible");
        }
        else
        {
            Debug.Log("PATH ES NULL");
        }
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
        initialTime = Time.realtimeSinceStartup;
        NavMeshHit navhit;
        Vector3 point;
        if(NavMesh.SamplePosition(owner.transform.position, out navhit, 0.5f, NavMesh.AllAreas))
        {
            point = navhit.position;
        }
        else
        {
            point = owner.transform.position;
        }

        NavMesh.CalculatePath(point, mov.metaPoint, NavMesh.AllAreas, path);
        
        mov.proximatePoint = new Vector3(-1, -1, -1);
        nscans = 0;
        auxScan = -1;
        faced = false;
        mov.behaviourIsRunning = true;
        mov.traceDone = false;
        mov.pathObstructed = false;
        mov.tooFar = false;
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
                sphere.transform.localScale = new Vector3(0.02f, 1f, 0.02f);
                (sphere.GetComponent<Renderer>()).material.color = new Color(color1, color2, color3);
                return point;
            }
        }
        Debug.Log("NO HAY PUNTO PROXIMO");
        return arrived;
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
        // Si no hay PATH -> Limpiar / Explorar en corto
        if(path.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("NO PATH FOUND");
            mov.calculateMetaPoint();
            mov.Stop(true);
            //mov.prision = true;
            return;
        }

        // Si el grafo es muy complejo -> Limpiar / Explorar en corto
        if (path.status != NavMeshPathStatus.PathComplete) // TODO pegado calculando path
        {

            Debug.Log("Calculating path...");
            float diff = Time.realtimeSinceStartup - initialTime;
            if ( diff > 5)
            {
                mov.calculateMetaPoint();
                mov.Stop(true);
                //mov.prision = true;
            }
            return;
        }

        if(path != null && path.corners.Length > 0 && mov.debug)
        {
            DrawPath();
        }

        float radius = mov.greenPoint != null ? 0.1f : 0.3f, angleThresh = 20;
        if (mov.proximatePoint.y == -1)
        {
            mov.proximatePoint = nextPoint(radius);
            if (mov.proximatePoint.y == -1) // TODO no hay punto proximo (Que hacer ? )
            {
                mov.Stop(true);
                mov.behaviourIsRunning = false;
                nscans = 0;
                mov.calculateMetaPoint();
                return;
            }
            initialDist = (mov.proximatePoint - owner.transform.position).magnitude;
        }

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

        /* Esperar a estar detenido y acumular N iteraciones */
        if (nscans >= 1 && faced)
        {
            if (initialDist > 1.5f)
            {
                mov.tooFar = true;
                return;
            }
            Debug.Log("Collision Raycasting");
            // Si hay algo en el medio RIP
            Debug.DrawRay(owner.transform.position, (mov.proximatePoint - owner.transform.position), Color.green,30);
            Vector3 directionVector = (mov.proximatePoint - owner.transform.position).normalized;
            float distance = (owner.transform.position - mov.proximatePoint).magnitude;
            bool ray1 = Physics.Raycast(owner.transform.position, directionVector, distance, LayerMask.GetMask("Ghobstacles"));
            bool ray2 = Physics.Raycast(owner.transform.position, Quaternion.AngleAxis(mov.robot.FOV, Vector3.up) * directionVector, distance, LayerMask.GetMask("Ghobstacles"));
            bool ray3 = Physics.Raycast(owner.transform.position, Quaternion.AngleAxis(-mov.robot.FOV, Vector3.up) * directionVector, distance, LayerMask.GetMask("Ghobstacles"));
            if (mov.debug)
            {
                Debug.DrawRay(owner.transform.position, directionVector * distance, Color.yellow, 10);
                Debug.DrawRay(owner.transform.position, Quaternion.AngleAxis(mov.robot.FOV, Vector3.up) * directionVector * distance, Color.yellow, 10);
                Debug.DrawRay(owner.transform.position, Quaternion.AngleAxis(-mov.robot.FOV, Vector3.up) * directionVector * distance, Color.yellow, 10);
            }

            if (ray1 || ray2 || ray3) 
            {
                Debug.Log("Path Obstructed");
                mov.pathObstructed = true;
            }
            else
            {
                Debug.Log("All clear");

                /*if (initialDist >= naiv.maxDistance / naiv.scale)
                {
                    mov.proximatePoint = (mov.proximatePoint - owner.transform.position).normalized * ((naiv.maxDistance / naiv.scale)*0.75f);
                }*/

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
