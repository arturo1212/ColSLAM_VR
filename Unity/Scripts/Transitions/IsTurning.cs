using UnityEngine;
public class IsTurning: Transition
{
    Movement mov;
    public IsTurning(State origin, State target) : base(origin, target)
    {
        mov = origin.owner.GetComponent<Movement>();
    }

    public override bool Eval()
    {
        //Debug.Log("Me mueven el piso "+(mov.turningRight || mov.turningLeft).ToString());
        return mov.turningRight || mov.turningLeft;
    }
}
