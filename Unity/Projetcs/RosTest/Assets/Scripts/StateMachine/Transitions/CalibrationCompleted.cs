
public class CalibrationCompleted : Transition
{
    private NaiveMapping naiveMapper;

    public CalibrationCompleted(State origin, State target) : base(origin,target)
    {
        naiveMapper = origin.owner.GetComponent<NaiveMapping>();
    }

    public override bool Eval()
    {
        return naiveMapper.arduinoSubscription_id != -1;
    }

}
