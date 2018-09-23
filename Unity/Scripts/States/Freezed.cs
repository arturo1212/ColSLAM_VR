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
        odo.diffRot += AngleHelpers.angleDifference(prevRotation, odo.gyro_reading); // Calcular diff
        odo.diffRot = AngleHelpers.angleToPositive(odo.diffRot);
    }
}
