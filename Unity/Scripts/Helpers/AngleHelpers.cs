using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleHelpers : MonoBehaviour {

    // Use this for initialization
    public static float angleDifference(float a, float b)
    {
        float result = a - b;
        result = result > 180 ? 360 - result : result;
        return a - b;
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
        float result = a < 0 ? 360 + a : a;
        return result;
    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
