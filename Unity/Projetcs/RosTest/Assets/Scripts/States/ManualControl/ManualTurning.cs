
using UnityEngine;

public class ManualTurning : State
{
    Movement mov;
    Odometry odo;
    public ManualTurning(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
    }

    public override void Circunloquio()
    {
        odo = owner.GetComponent<Odometry>();
        odo.useGyro = true;

    }

    public override void Colofon()
    {
        odo.useGyro = false;

    }

    public override void Execute()
    {
        mov.WASD();
        // Aplicar solo lo que aplique para rotacion (no traslacion) de Odometry
    }
}
