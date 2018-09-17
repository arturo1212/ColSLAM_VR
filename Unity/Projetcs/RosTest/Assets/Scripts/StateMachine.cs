using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour {

    public List<State> states;
    public List<Transition> transitions;
    public State currentState;

    private Movement move;
    public float targetRot=999, tresh;


    // Use this for initialization
    void Start () {
        Freezed freezedState = new Freezed();
        Turning turningState = new Turning();
        Going goingState = new Going();
        //states.Add(freezedState);
        //states.Add(turningState);
        //states.Add(goingState);
        move = GetComponent<Movement>();

    }

    public State EvalTransitions()
    {
        State targetState = currentState;
        foreach(Transition t in transitions)
        {
            if(t.origin == currentState)
            {
                return targetState = t.Eval();
            }
        }

        return targetState;
    }
	
    void Face()
    {
        if (targetRot == 999) return;
        float myrotation = transform.rotation.eulerAngles[1];
        float diffRot;
        if (Mathf.Abs(targetRot - myrotation) < Mathf.Abs(targetRot + 360 - myrotation))
        {
            diffRot = targetRot - myrotation;
        }
        else
        {
            diffRot = targetRot + 360 - myrotation;
        }
        if (Mathf.Abs(diffRot) > tresh)
        {
            move.facing = true;
            if (diffRot > 0)
            {
                move.TurnRight();
            }
            else
            {
                move.TurnLeft();
            }
        }
        else
        {
            move.facing = false;
            move.Stop();
        }
    }

	// Update is called once per frame
	void Update () {
        //currentState = EvalTransitions();
        //currentState.Execute();
        Face();
	}
}
