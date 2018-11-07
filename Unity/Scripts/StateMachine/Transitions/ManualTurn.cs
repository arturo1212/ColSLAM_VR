using UnityEngine;
public class ManualTurn : Transition
{
    public ManualTurn(State origin, State target) : base(origin, target)
    {
    }

    public override bool Eval()
    {
        return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    }
}