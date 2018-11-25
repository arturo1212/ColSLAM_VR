using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamScreen : MonoBehaviour {

    public GameObject screen;
    public GameObject activeRobot;
    NaiveMapping naiv;
    Texture2D tex;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (activeRobot != null)
        {
            naiv = activeRobot.GetComponent<NaiveMapping>();
            if(naiv.imageBytes!=null && naiv.imageBytes.Length != 0)
            {
                Destroy(tex);
                tex = new Texture2D(640, 480);
                tex.LoadImage(naiv.imageBytes);
                screen.GetComponent<Renderer>().material.mainTexture = tex;
            }
        }
	}
}
