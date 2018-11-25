using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerData : MonoBehaviour
{

    public SteamVR_TrackedObject trackedObj;

    public bool touchPadPressed=false, triggerPressed=false, touchPadPressUp=false, triggerPressUp=false, touchPadTouched=false;

    public Vector2 touchPad;

    public SteamVR_Controller.Device Controller;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    // Use this for initialization
    void Start()
    {
        Controller = SteamVR_Controller.Input((int)trackedObj.index);
    }

    private void FixedUpdate()
    {
        Controller = SteamVR_Controller.Input((int)trackedObj.index);
        touchPad = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
        touchPadPressed = Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
        triggerPressed = Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger);
        touchPadPressUp = Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
        triggerPressUp = Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger);
        touchPadTouched = Controller.GetTouch(SteamVR_Controller.ButtonMask.Axis0);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
