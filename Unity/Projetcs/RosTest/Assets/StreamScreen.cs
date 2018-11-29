using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamScreen : MonoBehaviour {

    public GameObject screen;
    public GameObject activeRobot;
    NaiveMapping naiv;
    NaiveClaw naiv_claw;
    Texture2D tex;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (activeRobot != null)
        {
            if (!activeRobot.transform.parent.name.Contains("CLAW"))
            {
                naiv = activeRobot.GetComponent<NaiveMapping>();
                if (naiv.imageBytes != null && naiv.imageBytes.Length != 0)
                {
                    Destroy(tex);
                    tex = new Texture2D(640, 480);
                    tex.LoadImage(naiv.imageBytes);
                    screen.GetComponent<Renderer>().material.mainTexture = tex;
                }
            }
            else
            {
                naiv_claw = activeRobot.GetComponent<NaiveClaw>();
                if (naiv_claw.imageBytes != null && naiv_claw.imageBytes.Length != 0)
                {
                    Destroy(tex);
                    tex = new Texture2D(640, 480);
                    tex.LoadImage(naiv_claw.imageBytes);
                    screen.GetComponent<Renderer>().material.mainTexture = tex;
                }
            }
            
        }
	}
}
