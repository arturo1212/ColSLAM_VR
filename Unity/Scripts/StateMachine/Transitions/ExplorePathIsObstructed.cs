using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorePathIsObstructed : Transition
{

    Movement mov;
    public ExplorePathIsObstructed(State origin, State target) : base(origin, target)
    {
        mov = origin.owner.GetComponent<Movement>();
    }

    public override bool Eval()
    {
        //Debug.Log("Me setearon el target "+(mov.clickedPoint != null).ToString());
        return mov.pathObstructed;
    }
}
