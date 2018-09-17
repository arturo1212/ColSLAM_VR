using System;

public abstract class Transition
{
    public State target, origin;

	public Transition(State target, State origin)
	{
        this.origin = origin;
        this.target = target;
	}

    public abstract State Eval();
}
