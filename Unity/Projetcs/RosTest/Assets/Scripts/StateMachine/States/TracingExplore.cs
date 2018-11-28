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

    bool faced = false, metaFaced, pathCalcStart = false, metaScan = false;
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

        if (mov.greenPoint != null)
        {
            mov.calculateMetaPoint();
        }
        mov.proximatePoint = new Vector3(-1, -1, -1);
        nscans = 0;
        auxScan = -1;
        faced = false;
        metaFaced = false;
        metaScan = false;
        pathCalcStart = false;
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
        // Orientacion hacia el metapunto
        if (!metaFaced)
        {
            SteeringBehaviours.Face(mov, mov.metaPoint, 20f, 0.25f, true);
            metaFaced = !mov.facing;
            return;
        }

        // Escaneos hacia el metapunto
        if (metaFaced && nscans < 1 && !metaScan)
        {
            mov.Stop();
            Debug.Log("Escaneando Luego del MetaFace");
            updateNScans();
            return;
        }
        else if(!metaScan)
        {
            metaScan = nscans >= 1;
            return;
        }
        // Calcular camino hacia el metapunto
        if (!pathCalcStart)
        {
            NavMeshHit navhit;
            Vector3 point, destiny;
            if (NavMesh.SamplePosition(owner.transform.position, out navhit, 0.5f, NavMesh.AllAreas))
            {
                point = navhit.position;
            }
            else
            {
                Debug.Log("ELSE DE LA PERDICION LEER CODIGO");
                point = owner.transform.position;
            }

            NavMeshHit navhit2;
            if (NavMesh.SamplePosition(mov.metaPoint, out navhit2, 0.5f, NavMesh.AllAreas))
            {
                destiny = navhit2.position;
            }
            else
            {
                Debug.Log("ELSE DE LA PERDICION LEER CODIGO");
                destiny = mov.metaPoint;
            }

            initialTime = Time.realtimeSinceStartup;
            NavMesh.CalculatePath(point, destiny, NavMesh.AllAreas, path);
            pathCalcStart = true;
        }

        float diff = Time.realtimeSinceStartup - initialTime;
        if (path.status == NavMeshPathStatus.PathInvalid || (path.status != NavMeshPathStatus.PathComplete && diff > 5 ) )
        {
            Debug.Log("NO PATH FOUND");
            //Cleaning
            mov.prision = true;
            return;
        }

        if(path != null && path.corners.Length > 0 && mov.debug)
        {
            DrawPath();
        }

        float radius = mov.greenPoint != null ? 0.1f : 0.25f, angleThresh = 20;

        // Obtener punto del camino generado.
        if (mov.proximatePoint.y == -1)
        {
            mov.proximatePoint = nextPoint(0.2f);
            if (mov.proximatePoint.y == -1) // TODO no hay punto proximo (Que hacer ? )
            {
                mov.prision = true;
                return;
            }
            initialDist = (mov.proximatePoint - owner.transform.position).magnitude;
        }

        if (!faced)
        {
            SteeringBehaviours.Face(mov, mov.proximatePoint, angleThresh, radius, true);
            faced = !mov.facing;
            nscans = 0;
            auxScan = -1;
        }

        if (faced)
        {
            Debug.Log("Escaneando");
            updateNScans();
        }

        /* Esperar a estar detenido y acumular N iteraciones */
        if (nscans >= 1 && faced)
        {
            if (initialDist > naiv.maxDistance)
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
            if (mov.debug)
            {
                Debug.DrawRay(owner.transform.position, directionVector * distance, Color.yellow, 10);
            }

            if (ray1 ) 
            {
                Debug.Log("Path Obstructed");
                //Recordar lo del contador. Pero provisionalmente nos vamos a cleaning
                //mov.calculateMetaPoint();
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
