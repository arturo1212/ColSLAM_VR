
using UnityEngine;

public class ManualGoing : State
{
    Movement mov;
    Odometry odo;
    public ManualGoing(GameObject owner) : base(owner)
    {
        mov = mov.GetComponent<Movement>();
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
        //Aplicar el modelo de Odometry

    }
}
