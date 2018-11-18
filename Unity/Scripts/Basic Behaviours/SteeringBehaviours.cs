using UnityEngine;

public static class SteeringBehaviours{

	// Use this for initialization
	static void Start () {
		
	}
	
	// Update is called once per frame
	static void Update () {
		
	}

    public static void Face(Movement move, float targetRot, float tresh, bool notify=false)
    {
        float myrotation = move.transform.rotation.eulerAngles[1];
        float diffRot;
        //Debug.Log("TargetRot "+targetRot);
        //Debug.Log("MyRotation " + myrotation);
        diffRot = Mathf.DeltaAngle(myrotation, targetRot);
        //Debug.Log("Deiffrot con modulo " + diffRot);
        
        if (Mathf.Abs(diffRot) > tresh)
        {
            //Debug.Log("Rotandito");

            if (notify)
            {
                move.facing = true;
            }
            if (diffRot > 0)
            {
                move.TurnRight();
            }
            else
            {
                move.TurnLeft();
            }
        }
        else
        {
            //Debug.Log("Ya lo miro");
            if (notify)
            {
                move.facing = false;
            }
            //move.stopped = true;
            move.Stop();
        }
    }


    public static void Face(Movement move, Vector3 target, float tresh, bool notify=false)
    {
        float myRot = move.transform.rotation.eulerAngles[1];
        //Debug.Log("Facing point: " + target);
        Face(move, AngleHelpers.angleToPositive(AngleHelpers.angleToLookTo(move.transform, target) + myRot), tresh, notify);
    }

    // La forma en la que esta funcion controla el robot esta deprecada
    public static void ForwardFace(Movement move, float targetRot, float tresh)
    {
        Debug.Log("Forwardfacing");
        float myrotation = move.transform.rotation.eulerAngles[1];
        float diffRot;
        if (Mathf.Abs(targetRot - myrotation) > 180)
        {

            diffRot = (360 - (targetRot - myrotation)) * ((targetRot > myrotation) ? -1 : 1);
        }
        else
        {
            diffRot = targetRot - myrotation;
        }
        if (Mathf.Abs(diffRot) > tresh)
        {
            move.behaviourIsRunning = true;
            if (diffRot > 0)
            {
                //move.Send_motors_pwm(move.LVelocity,0);
            }
            else
            {
                //move.Send_motors_pwm(0, move.RVelocity);
            }
        }
        else
        {
            move.behaviourIsRunning = false;
        }
    }

    public static void GoToGoal(Movement mov, Vector3 goal, float radius, float facingThresh, bool notify=false)
    {
        //Debug.Log("Yendo al goal");
        Vector3 targetDir = goal - mov.transform.position;
        float deltaRot = Vector3.SignedAngle(mov.transform.forward, targetDir.normalized, Vector3.up);
        //Debug.Log("DeltaRot " + deltaRot);
        if (Mathf.Abs(deltaRot) > facingThresh && targetDir.magnitude > radius )
        {
            //Debug.Log("Face del gotogoal");
            if (notify)
            {
                mov.goingToGoal = true;
            }
            Face(mov, goal, facingThresh);
        }
        else
        {
            //Debug.Log("Quiza me mueva, me faltan "+ targetDir.magnitude);
            if (targetDir.magnitude > radius)
            {
                //Debug.Log("Mevoamove");
                if (notify)
                {
                    mov.goingToGoal = true;
                }
                mov.GoForward();
            }
            else
            {
                //Debug.Log("LLEGUE!");
                if (notify)
                {
                    mov.goingToGoal = false;
                }
                mov.Stop();
            }
        }
        

    }
}
