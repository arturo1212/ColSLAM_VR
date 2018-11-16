﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShouldClean : Transition
{

    Movement mov;
    public ShouldClean(State origin, State target) : base(origin, target)
    {
        mov = origin.owner.GetComponent<Movement>();
    }

    public override bool Eval()
    {
        //Debug.Log("Me setearon el target "+(mov.metaPoint != null).ToString());
        return mov.prision;
    }
}
