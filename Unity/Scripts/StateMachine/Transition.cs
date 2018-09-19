public abstract class Transition
{
    public State target, origin;

	public Transition(State origin, State target )
	{
        this.origin = origin;
        this.target = target;
	}

    public abstract bool Eval();
}
