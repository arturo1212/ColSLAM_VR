using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawControl : MonoBehaviour {
    RosSocket rosSocket;
    public string robotIP = "ws://192.168.1.105:9090";  // IP del robot.
    public string Topic = "claw";
    public int publicationId;

    void Start()
    {
        rosSocket = new RosSocket(robotIP);
        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
    }

    void Update () {
        StandardString msg = new StandardString();
        msg.data = "lala";
        rosSocket.Publish(publicationId, msg);
    }
}
