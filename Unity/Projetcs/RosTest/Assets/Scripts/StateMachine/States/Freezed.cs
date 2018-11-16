using UnityEngine;

public class Freezed: State
{
    public float prevRotation;
    private NaiveMapping rosComm;
    Movement mov;
    float prev_diffrot; 
	public Freezed(GameObject owner ) : base(owner)
	{
        rosComm = owner.GetComponent<NaiveMapping>();
        mov = owner.GetComponent<Movement>(); 
	}

    public override void Circunloquio()
    {
        //Debug.Log("Me paro");
        /*odo = owner.GetComponent<Odometry>();
        prev_diffrot = odo.diffRot;

        odo.useGyro = false;
        prevRotation = rosComm.rotation_robot;
        odo.appliedRotation = prevRotation;
        */
    }

    public override void Execute()
    {
        /*
        odo.diffRot = prev_diffrot + Mathf.DeltaAngle(prevRotation, odo.gyro_reading);
        if(Mathf.Abs(Mathf.DeltaAngle(prevRotation, rosComm.rotation_robot)) > 8)
        {
            odo.appliedRotation = rosComm.rotation_robot;
        }*/
        //Enviar nada a los motores
        // No hacer nada con la rotacion ni con traslacion (no aplicar modelo de odometria)
    }

    public override void Colofon()
    {
        mov.stopped = false;
        //Debug.Log("Me dejo de parar");
        //Debug.Log("Calculada " +odo.diffRot);
        //odo.diffRot = AngleHelpers.angleToPositive(odo.diffRot);
        //Debug.Log("Mappeada " + odo.diffRot);
    }
}
