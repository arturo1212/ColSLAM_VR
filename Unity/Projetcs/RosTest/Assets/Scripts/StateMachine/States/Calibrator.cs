using UnityEngine;


public class Calibrator: State
{
    Movement mov;
	public Calibrator(GameObject owner) : base(owner)
	{
        mov = owner.GetComponent<Movement>();
	}

    public override void Circunloquio()
    {

    }

    public override void Execute()
    {
        mov.calculateMetaPoint();
    }
    
    public override void Colofon()
    {
        //owner.AddComponent<Odometry>().appliedRotation = owner.GetComponent<NaiveMapping>().rotation_robot;
    }

}
