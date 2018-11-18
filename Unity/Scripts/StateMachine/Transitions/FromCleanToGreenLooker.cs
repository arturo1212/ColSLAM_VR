using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromCleanToGreenLooker : Transition
{

    Movement mov;
    NaiveMapping naiv;
    public FromCleanToGreenLooker(State origin, State target) : base(origin, target)
    {
        mov = origin.owner.GetComponent<Movement>();
        naiv = origin.owner.GetComponent<NaiveMapping>();
    }

    public override bool Eval()
    {
        //Debug.Log("Me setearon el target "+(mov.metaPoint != null).ToString());
        return mov.greenPoint != null ;
    }
}
