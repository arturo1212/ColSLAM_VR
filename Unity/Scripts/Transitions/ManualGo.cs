using UnityEngine;
public class ManualGo : Transition
{
    public ManualGo(State origin, State target) : base(origin, target)
    {
    }

    public override bool Eval()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
    }
}