using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPointSet : Transition
{
    
    Movement mov;
    public GoalPointSet(State origin, State target) : base(origin, target)
    {
        mov = origin.owner.GetComponent<Movement>();
    }

    public override bool Eval()
    {
        //Debug.Log("Me setearon el target "+(mov.clickedPoint != null).ToString());
        return mov.clickedPoint != null;
    }
}
