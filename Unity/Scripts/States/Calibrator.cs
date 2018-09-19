using UnityEngine;


public class Calibrator: State
{
	public Calibrator(GameObject owner) : base(owner)
	{
        
	}

    public override void Circunloquio()
    {

    }

    public override void Execute()
    {

    }
    
    public override void Colofon()
    {
        owner.AddComponent<Odometry>();
    }

}
