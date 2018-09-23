using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour {

    public List<State> states = new List<State>();
    public List<Transition> transitions = new List<Transition>();
    public State currentState;

    private Movement move;
    private NaiveMapping naiveMapper;
    public float targetRot=999, tresh;


    // Use this for initialization
    void Start () {
        Calibrator calibratorState = new Calibrator(gameObject);
        Freezed freezedState = new Freezed(gameObject);
        Turning turningState = new Turning(gameObject);
        Going goingState = new Going(gameObject);
        GoingToGoal goToGoalState = new GoingToGoal(gameObject);
        ManualTurning manualTurningState = new ManualTurning(gameObject);
        ManualGoing manualGoingState = new ManualGoing(gameObject);


        states.Add(calibratorState);
        states.Add(freezedState);
        states.Add(turningState);
        currentState = states[0];

        transitions.Add(new CalibrationCompleted(calibratorState, freezedState));
        transitions.Add(new Stopped(turningState, freezedState));
        transitions.Add(new FacingTargetSet(freezedState, goToGoalState));
        transitions.Add(new Stopped(goToGoalState, freezedState));
        transitions.Add(new ManualTurn(freezedState, manualTurningState));
        transitions.Add(new ManualGo(freezedState, manualGoingState));
        // TODO Hace falta salir de otros estados a los manuales ??

        move = GetComponent<Movement>();
        naiveMapper = GetComponent<NaiveMapping>();

    }

    public State EvalTransitions()
    {
        foreach(Transition t in transitions)
        {
            if(t.origin == currentState)
            {
                if (t.Eval())
                {
                    currentState.Colofon();
                    t.target.Circunloquio();
                    return t.target;
                }

            }
        }
        return currentState;
    }
	
	// Update is called once per frame
	void Update () {
        currentState = EvalTransitions();
        currentState.Execute();
	}
}
