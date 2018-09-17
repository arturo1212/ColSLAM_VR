using System;

public class Freezed: State
{
	public Freezed()
	{
	}

    public override void Execute()
    {
        //Enviar nada a los motores
        // No hacer nada con la rotacion ni con traslacion (no aplicar modelo de odometria)
    }
}
