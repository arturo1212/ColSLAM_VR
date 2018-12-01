using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour {

    public List<State> states = new List<State>();
    public List<Transition> transitions = new List<Transition>();
    public State currentState, prevState;

    private Movement move;
    private NaiveMapping naiveMapper;
    public string current;

    // Use this for initialization
    void Start () {
        Calibrator calibratorState = new Calibrator(gameObject);
        Freezed freezedState = new Freezed(gameObject);
        Turning turningState = new Turning(gameObject);
        Going goingState = new Going(gameObject);
        GoingToGoal goToGoalState = new GoingToGoal(gameObject);
        ManualTurning manualTurningState = new ManualTurning(gameObject);
        ManualGoing manualGoingState = new ManualGoing(gameObject);
        TracingExplore tracingExploreState = new TracingExplore(gameObject);
        ExploreMove exploreMoveState = new ExploreMove(gameObject);
        Greenlooker greenLookerState = new Greenlooker(gameObject);
        Cleaning cleaningState = new Cleaning(gameObject);
        Explore exploreState = new Explore(gameObject);

        states.Add(calibratorState);
        //states.Add(exploreMoveState);
        //states.Add(tracingExploreState);
        currentState = states[0];

        //FALTA PASAR A LO DEL VERDE DESDE EXPLORE DEL TRACING EXPLORE

        transitions.Add(new CalibrationCompleted(calibratorState, freezedState));
        transitions.Add(new GoalPointSet(freezedState, tracingExploreState));
        transitions.Add(new ExplorePathIsObstructed(tracingExploreState, exploreState));
        transitions.Add(new ExplorePathIsObstructed(exploreState, exploreState));
        transitions.Add(new ExploreTraced(tracingExploreState, exploreMoveState));
        transitions.Add(new ExploreTraced(exploreState, exploreMoveState));
        transitions.Add(new Stopped(exploreMoveState, freezedState));
        //transitions.Add(new Stopped(tracingExploreState, freezedState));
        transitions.Add(new Stopped(greenLookerState, freezedState));
        //transitions.Add(new Stopped(cleaningState, freezedState));
        transitions.Add(new ShouldLookGreen(exploreMoveState, greenLookerState));
        transitions.Add(new ShouldClean(tracingExploreState, exploreState));
        //transitions.Add(new FromCleanToGreenLooker(cleaningState, greenLookerState));
        
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
                    //Debug.Log("Transicionando");
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
        if (naiveMapper.markerFound)
        {
            //move.Stop();
            //return;
        }
        currentState = EvalTransitions();
        currentState.Execute();
        current = currentState.ToString();
        if(prevState != currentState)
        {
            prevState = currentState;
            Debug.Log(currentState);
        }

    }
}
