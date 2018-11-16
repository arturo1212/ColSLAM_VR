

public class Stopped: Transition
{
    Movement mov;
    public Stopped(State origin, State target): base(origin,target)
    {
        mov = origin.owner.GetComponent<Movement>();
    }

    public override bool Eval()
    {
        return mov.stopped;
    }
}
