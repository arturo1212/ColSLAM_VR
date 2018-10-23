using UnityEngine;

public static class SteeringBehaviours{

	// Use this for initialization
	static void Start () {
		
	}
	
	// Update is called once per frame
	static void Update () {
		
	}

    public static void Face(Movement move, float targetRot, float tresh)
    {
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
            move.facing = true;
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
            move.facing = false;
            move.Stop();
        }
    }

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
            move.facing = true;
            if (diffRot > 0)
            {
                move.send_motors_pwm(move.LVelocity,0);
            }
            else
            {
                move.send_motors_pwm(0, move.RVelocity);
            }
        }
        else
        {
            move.facing = false;
        }
    }

    public static void GoToGoal(Movement mov, Vector3 goal, float radius, float facingThresh)
    {
        Debug.Log("Yendo al goal");
        Vector3 targetDir = goal - mov.transform.position;
        float deltaRot = Vector3.SignedAngle(mov.transform.forward, targetDir, Vector3.up);
        //Debug.Log("DeltaRot " + deltaRot);
        if (Mathf.Abs(deltaRot) > facingThresh)
        {
            deltaRot += mov.transform.rotation.eulerAngles[1];
            deltaRot = AngleHelpers.angleToPositive(deltaRot);
            Debug.Log("A rotar a" + deltaRot);
            Face(mov, deltaRot, facingThresh);
        }
        else
        {
            Debug.Log("Quiza me mueva, me faltan "+ targetDir.magnitude);
            if (targetDir.magnitude > radius)
            {
                Debug.Log("Mevoamove");
                mov.GoForward();
            }
            else
            {
                Debug.Log("LLEGUE!");
                mov.Stop();
            }
        }
        

    }
}
