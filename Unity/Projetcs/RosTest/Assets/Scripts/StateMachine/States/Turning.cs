using System;
using UnityEngine;

public class Turning: State
{
    Movement mov;
    Odometry odo;
	public Turning(GameObject owner):base(owner)
	{
        mov = owner.GetComponent<Movement>();
    }

    public override void Circunloquio()
    {
        Debug.Log("Entrando en Turning");
        odo = owner.GetComponent<Odometry>();
        odo.useGyro = true;
    }

    public override void Execute() {
        // Aplicar solo lo que aplique para rotacion (no traslacion) de Odometry
        Vector3 targetDir = (Vector3)mov.metaPoint - owner.transform.position;
        float targetRot = Vector3.SignedAngle(owner.transform.forward, targetDir, Vector3.up) + owner.transform.rotation.eulerAngles[1];
        targetRot = AngleHelpers.angleToPositive(targetRot);
        Debug.Log("Quiero ver a " +targetRot);
        SteeringBehaviours.Face(mov, targetRot, 5);
    }

    public override void Colofon()
    {
        mov.metaPoint = null;
        odo.useGyro = false;
    }
}
