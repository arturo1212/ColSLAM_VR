﻿using UnityEngine;

public class Freezed: State
{
    public float prevRotation;
    private Odometry odo;
    private NaiveMapping rosComm;
    float prev_diffrot; 
	public Freezed(GameObject owner ) : base(owner)
	{
        rosComm = owner.GetComponent<NaiveMapping>();
	}

    public override void Circunloquio()
    {
        odo = owner.GetComponent<Odometry>();
        prev_diffrot = odo.diffRot;

        odo.useGyro = false;
        prevRotation = rosComm.rotation_robot;
        odo.appliedRotation = prevRotation;
    }

    public override void Execute()
    {
        odo.diffRot = prev_diffrot + Mathf.DeltaAngle(prevRotation, odo.gyro_reading);
        //Enviar nada a los motores
        // No hacer nada con la rotacion ni con traslacion (no aplicar modelo de odometria)
    }

    public override void Colofon()
    {
        Debug.Log("Calculada " +odo.diffRot);
        //odo.diffRot = AngleHelpers.angleToPositive(odo.diffRot);
        Debug.Log("Mappeada " + odo.diffRot);
    }
}
