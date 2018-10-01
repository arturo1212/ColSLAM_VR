using UnityEngine;

public class Freezed: State
{
    public float prevRotation;
    private Odometry odo;
    private NaiveMapping rosComm;

	public Freezed(GameObject owner ) : base(owner)
	{
        rosComm = owner.GetComponent<NaiveMapping>();
	}

    public override void Circunloquio()
    {
        odo = owner.GetComponent<Odometry>();
        odo.useGyro = false;
        prevRotation = rosComm.rotation_robot;
        odo.appliedRotation = prevRotation;
    }

    public override void Execute()
    {
        //Enviar nada a los motores
        // No hacer nada con la rotacion ni con traslacion (no aplicar modelo de odometria)
    }

    public override void Colofon()
    {
        odo.diffRot += Mathf.DeltaAngle(prevRotation, odo.gyro_reading); //AngleHelpers.angleDifference(prevRotation, odo.gyro_reading);
        Debug.Log("Calculada " +odo.diffRot);
        odo.diffRot = AngleHelpers.angleToPositive(odo.diffRot);
        Debug.Log("Mappeada " + odo.diffRot);
    }
}
