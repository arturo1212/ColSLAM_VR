using UnityEngine;

public abstract class State
{
    public GameObject owner;

    public State(GameObject owner)
    {
        this.owner = owner;
    }

    public abstract void Circunloquio();
    public abstract void Execute();
    public abstract void Colofon();
}
