using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveBall : MonoBehaviour {
	public float distance;
	public int maxDistance = 500;
	bool back = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame

	void Update() {
		float scaled = (distance / maxDistance) * 10;
		Vector3 offset = new Vector3 (0, 0, scaled); 
		transform.position = offset;
		/*
		if (distance <= maxDistance && !back) {
			back = false;
			distance++;
		} else if (distance > 0) {
			back = true;
			distance--;			
		} else {
			back = false;
		}
		*/

	}
}
