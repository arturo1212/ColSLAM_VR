using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configuration : MonoBehaviour {

    public List<GameObject> agents; // Agentes.
    public GameObject selectedAgent;       
    int separation = 12;         // Separacion entre entornos.
    public int index = 0, agent_count = 0;     // Varible auxiliar para mantener el ultimo tamano de lista.

	// Use this for initialization
	void Start () {
		for(int i =0; i < agents.Count; i++)
        {
            GameObject dummy = Instantiate(agents[i], new Vector3(i*separation, 0, 0), Quaternion.identity); // Instanciar prefab[i] en x=i*separation
            print(dummy.transform.position);
        }
        index = 0;
        selectedAgent = agents[index];
        agent_count = agents.Count;
	}
	
	// Update is called once per frame
    void checkAddedAgent()
    {
        if (agents.Count > agent_count)
        {
            for (int i = agent_count; i < agents.Count; i++)
            {
                GameObject dummy = Instantiate(agents[i], new Vector3(i * separation, 0, 0), Quaternion.identity); // Instanciar prefab[i] en x=i*separation
            }
            agent_count = agents.Count;
        }
    }

    void changeCameraFocus()
    {
        index = (index + 1) % agents.Count;
        selectedAgent = agents[index];
        var newPosition = new Vector3(index*separation, 5f, 0f);
        Camera.main.transform.position = newPosition;
    }

    void Update () {
        checkAddedAgent();
        if (Input.GetKeyDown(KeyCode.E))
        {
            changeCameraFocus();
        }
    }
}
