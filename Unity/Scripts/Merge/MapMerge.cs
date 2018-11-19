using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapMerge : MonoBehaviour {
    public List<GameObject> fields = new List<GameObject>();
    public GameObject axis_helper;
    public int aux_count = 0;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var fs = GameObject.FindGameObjectsWithTag("Fields").ToList();
        if (fs.Count > aux_count)
        {
            fields = fs;
            aux_count = fields.Count;
        }
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
                    MapOverlapping(g, f, Pivot_3Point);
                    // Add Robot to map
                    fields.Remove(g);
                    //Destroy(g);
                    return;
                }
                else if(markersA.Count >= 1)
                {
                    Debug.Log("MERGEANDO");
                    MapOverlapping(g, f, Pivot_1Point);
                    
                    // Add Robot to map
                    var robot = GetChildObject(g.transform, "Robot")[0]; // GENERALIZAR
                    robot.GetComponent<Movement>().metaPoints = GetChildObject(f.transform, "MetaPoints");
                    
                    // Clean fields
                    fields.Remove(g);
                    Destroy(g);
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

        // Obtener marcas en cada robot y ver las comunes.
        List<GameObject> markersA = GetChildObject(fieldA.transform, "Marker");
        List<GameObject> markersB = GetChildObject(fieldB.transform, "Marker");
        List<string> commonM  = markersA.Select(f => f.name).Intersect(markersB.Select(b => b.name)).ToList();  // Obtener todos los markers encontrados por cada uno.
        commonMarkersA = markersA.Where(f => commonM.Contains(f.name)).ToList();    // Obtener los markers de cada robot.
        commonMarkersB = markersB.Where(f => commonM.Contains(f.name)).ToList();
        // (FALTA) Ordenar las coincidencias
        return new Tuple<List<GameObject>, List<GameObject>>(commonMarkersA, commonMarkersB);
    }

    void MergePosition(GameObject fieldA, GameObject fieldB, GameObject pivotA, GameObject pivotB)
    {
        // Assert: Las rotaciones estan en comun && pivotA == pivotB
        fieldA.transform.parent = pivotA.transform; // Parentezco para posicionamiento.
        fieldB.transform.parent = pivotB.transform;
        pivotA.transform.position = pivotB.transform.position;

        // Recrear obstaculos.
        List<GameObject> obstaclesA = GetChildObject(fieldA.transform, "Obstacles");
        List<GameObject> obstaclesB = GetChildObject(fieldB.transform, "Obstacles");
        GridGenerator gg = fieldB.GetComponent<GridGenerator>();
        foreach (GameObject obstacle in obstaclesA)
        {
            gg.createCube(obstacle.transform.position);
        }
        // Eliminar los pivotes.
        fieldA.transform.parent = null; // Parentezco para posicionamiento.
        fieldB.transform.parent = null;
        Destroy(pivotA);
        Destroy(pivotB);
    }

    void MapOverlapping(GameObject fieldA, GameObject fieldB, Func<GameObject, GameObject, Tuple<GameObject,GameObject>> getPivots)
    {
        // getPivots selecciona los pivotes y realiza las rotaciones pertinentes.
        //Obtener el centro entre dos marcas y definir eje de rotacion
        Tuple <GameObject,GameObject> pivots = getPivots(fieldA, fieldB);
        GameObject axisA = pivots.Item1, axisB = pivots.Item2;
        MergePosition(fieldA, fieldB, axisA, axisB);    // Se puede pasar como argumento tambien.
    }

    #region Hallar pivote
    Tuple<GameObject, GameObject> Pivot_3Point(GameObject fieldA, GameObject fieldB)
    {
        var coincidences = marker_coincidences(fieldA, fieldB);
        var markersA = coincidences.Item1;
        var markersB = coincidences.Item2;

        //Obtener el centro entre dos marcas y definir eje de rotacion
        Vector3 centerA = markersA[0].transform.position + (markersA[1].transform.position - markersA[0].transform.position) / 2;
        Vector3 centerB = markersB[0].transform.position + (markersB[1].transform.position - markersB[0].transform.position) / 2;
        GameObject axisA = Instantiate(axis_helper, centerA, Quaternion.identity);
        GameObject axisB = Instantiate(axis_helper, centerB, Quaternion.identity);

        // Establecer orientacion de ambos ejes en la misma direccion.
        axisA.transform.LookAt(markersA[2].transform.position);
        axisB.transform.LookAt(markersB[2].transform.position);

        // Crear relaciones de parentezco para realizar la rotacion
        fieldA.transform.parent = axisA.transform;
        fieldB.transform.parent = axisB.transform;

        // Igualar rotacion y posicion de los planes
        axisA.transform.rotation = axisB.transform.rotation;
        fieldA.transform.parent = null; // Parentezco para posicionamiento.
        fieldB.transform.parent = null;
        return new Tuple<GameObject, GameObject>(axisA, axisB);
    }

    Tuple<GameObject, GameObject> Pivot_1Point(GameObject fieldA, GameObject fieldB)
    {
        var coincidences = marker_coincidences(fieldA, fieldB);
        var markersA = coincidences.Item1;
        var markersB = coincidences.Item2;

        //Obtener el centro entre dos marcas y definir eje de rotacion
        GameObject axisA = markersA[0];
        GameObject axisB = markersB[0];

        // Crear relaciones de parentezco para realizar la rotacion
        fieldA.transform.parent = axisA.transform;
        fieldB.transform.parent = axisB.transform;

        // Igualar rotacion y posicion de los planos
        axisA.transform.rotation = axisB.transform.rotation;
        return new Tuple<GameObject, GameObject>(axisA, axisB);
    }

    #endregion

    #region Helpers
    public static List<GameObject> GetChildObject(Transform parent, string _tag)
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
    #endregion

    #region Not used
    /* WORKING  */
    void MapOverlapping(GameObject a1, GameObject a2, GameObject a3, GameObject b1, GameObject b2, GameObject b3, GameObject fieldA, GameObject fieldB)
    {
        //Obtener el centro entre dos marcas y definir eje de rotacion
        Vector3 centerA = a1.transform.position + (a2.transform.position - a1.transform.position) / 2;
        Vector3 centerB = b1.transform.position + (b2.transform.position - b1.transform.position) / 2;
        GameObject axisA = Instantiate(axis_helper, centerA, Quaternion.identity);
        GameObject axisB = Instantiate(axis_helper, centerB, Quaternion.identity);

        // Establecer orientacion de ambos ejes en la misma direccion.
        axisA.transform.LookAt(a3.transform.position);
        axisB.transform.LookAt(b3.transform.position);

        // Crear relaciones de parentezco para realizar la rotacion
        fieldA.transform.parent = axisA.transform;
        fieldB.transform.parent = axisB.transform;

        // Igualar rotacion y posicion de los planes
        axisA.transform.rotation = axisB.transform.rotation;
        MergePosition(fieldA, fieldB, axisA, axisB);
    }
    #endregion
}
