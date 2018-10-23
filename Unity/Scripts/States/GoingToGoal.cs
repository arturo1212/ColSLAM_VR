using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoingToGoal : State
{
    Movement mov;
    public GoingToGoal(GameObject owner) : base(owner)
    {
        mov = owner.GetComponent<Movement>();
    }

    public override void Circunloquio()
    {
        Debug.Log("Quiero ir al goal");
        mov.stopped = false;
        mov.facing = true;

    }

    public override void Colofon()
    {
        mov.clickedPoint = null;
        mov.facing = false;

    }

    public override void Execute()
    {
        SteeringBehaviours.GoToGoal(mov, (Vector3)mov.clickedPoint, 0.1f, 10);
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
