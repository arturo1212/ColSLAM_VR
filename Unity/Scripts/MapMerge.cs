using MathNet.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapMerge : MonoBehaviour {
    public List<GameObject> fields = new List<GameObject>();
    public GameObject axis_helper;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        List<GameObject> aux = new List<GameObject>();
        foreach (GameObject g in fields)
        {
            aux.Add(g);
            foreach (GameObject f in fields.Except(aux))
            {
                var coincidences = marker_coincidences(g, f);
                var markersA = coincidences.Item1;
                var markersB = coincidences.Item2;
                if (markersA.Count >= 3)
                {
                    MapOverlapping(markersA[0], markersA[1], markersA[2], markersB[0], markersB[1], markersB[2], g, f);
                    fields.Remove(f);
                    Destroy(f);
                    return;
                }
            }
        }
    }

    /* Obtener todas las coincidencias entre 2 mapas */
    Tuple<List<GameObject>, List<GameObject>> marker_coincidences(GameObject fieldA, GameObject fieldB)
    {
        List<GameObject> commonMarkersA = new List<GameObject>();
        List<GameObject> commonMarkersB = new List<GameObject>();
        List<GameObject> markersA = GetChildObject(fieldA.transform, "Marker");
        List<GameObject> markersB = GetChildObject(fieldB.transform, "Marker");
        List<string> commonM  = markersA.Select(f => f.name).Intersect(markersB.Select(b => b.name)).ToList();
        commonMarkersA = markersA.Where(f => commonM.Contains(f.name)).ToList();
        commonMarkersB = markersB.Where(f => commonM.Contains(f.name)).ToList();
        return new Tuple<List<GameObject>, List<GameObject>>(commonMarkersA, commonMarkersB);
    }

    /*  */
    void MapOverlapping(GameObject a1, GameObject a2, GameObject a3, GameObject b1, GameObject b2, GameObject b3, GameObject fieldA, GameObject fieldB)
    {
        Vector3 centerA = a1.transform.position + (a2.transform.position - a1.transform.position) / 2;
        Vector3 centerB = b1.transform.position + (b2.transform.position - b1.transform.position) / 2;
        GameObject axisA = Instantiate(axis_helper, centerA, Quaternion.identity);
        GameObject axisB = Instantiate(axis_helper, centerB, Quaternion.identity);
        axisA.transform.LookAt(a3.transform.position);
        axisB.transform.LookAt(b3.transform.position);
        fieldA.transform.parent = axisA.transform;
        fieldB.transform.parent = axisB.transform;
        axisA.transform.rotation = axisB.transform.rotation;
        axisA.transform.position = axisB.transform.position;
        List<GameObject> obstaclesA = GetChildObject(fieldA.transform, "Obstacle");
        List<GameObject> obstaclesB = GetChildObject(fieldB.transform, "Obstacle");
        //Plane p = fieldB.GetComponent<Plane>();
        GridGenerator gg = fieldB.GetComponent<GridGenerator>();
        foreach (GameObject obstacle in obstaclesB)
        {

            //Vector3 point = p.ClosestPointOnPlane(obstacle.transform.position);
            gg.createCube(obstacle.transform.position);
        }
    }

    public List<GameObject> GetChildObject(Transform parent, string _tag)
    {
        List<GameObject> result = new List<GameObject>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == _tag)
            {
                result.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                GetChildObject(child, _tag);
            }
        }
        return result;
    }
}
