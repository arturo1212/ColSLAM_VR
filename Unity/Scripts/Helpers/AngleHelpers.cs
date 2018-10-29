using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleHelpers : MonoBehaviour {

    // Use this for initialization
    // Mathf.AngleDelta is the shit
    public static float angleDifference(float a, float b)
    {
        float result = a - b;
        result = result > 180 ? 360 - result : result;
        return a - b;
    }

    public static float angleToLookTo(Transform me, Vector3 goal)
    {
        Vector3 targetDir = goal - me.position;
        float diff = Vector3.SignedAngle(me.forward, targetDir, Vector3.up);
        //Debug.Log("angle to look"+diff);
        return diff;
    }

    public static float angleToPositive(float a)
    {
        float signo = a<0 ? -1f : 1f;
        float value = (Mathf.Abs(a) % 360)*signo;
        float result = value < 0 ? 360 + value : value;
        
        return result;
    }

    public static float Among360(float a)
    {
        float result = a < 0 ? 360 + a : a%360;
        return result;
    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
