using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {

    public float updateInterval = 0.5F;
    private double lastInterval;
    Dictionary<Vector3, GameObject> placed_cubes = new Dictionary<Vector3, GameObject>();
    Collider floorCollider;
    float step_x, step_z;
    public int rows = 10, columns = 10; // UTILIZAR VALORES PARES

    public void createGrid()
    {
        floorCollider = gameObject.GetComponent(typeof(Collider)) as Collider;
        Vector3 foorSize = new Vector3(floorCollider.bounds.size.x, floorCollider.bounds.size.z);
        print(floorCollider.bounds.center);
        //print(foorSize);

        step_x = floorCollider.bounds.size.x / rows;
        step_z = floorCollider.bounds.size.z / columns;

        float start_x = floorCollider.bounds.center.x - floorCollider.bounds.size.x / 2;
        float start_z = floorCollider.bounds.center.z - floorCollider.bounds.size.z / 2;

        for (float i = start_x; i <= start_x + floorCollider.bounds.size.x; i+=step_x )
        {
            LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer> () as LineRenderer;
            lineRenderer.transform.parent = transform;
            lineRenderer.useWorldSpace = false;
            lineRenderer.widthMultiplier = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, new Vector3(i, 0.01f, -floorCollider.bounds.size.z / 2));
            lineRenderer.SetPosition(1, new Vector3(i, 0.01f, floorCollider.bounds.size.z / 2));
        }

        for (float i = start_z; i <= start_z + floorCollider.bounds.size.z; i += step_z)
        {
            LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>() as LineRenderer;
            lineRenderer.transform.parent = transform;
            lineRenderer.useWorldSpace = false;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, new Vector3(-floorCollider.bounds.size.x / 2, 0.01f, i));
            lineRenderer.SetPosition(1, new Vector3(floorCollider.bounds.size.x / 2, 0.01f, i));
        }
    }

	// Use this for initialization
	void Start () {
        createGrid();
        lastInterval = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Clicked();
        }

        float timeNow = Time.realtimeSinceStartup;
        if (timeNow < lastInterval + updateInterval)
        {
            return;
        }
        lastInterval = timeNow;

        /* Actualizar todos los cubos cada N segundos*/
        foreach (Vector3 point in placed_cubes.Keys)
        {
            GameObject cube = placed_cubes[point];
            Color c = cube.GetComponent<Renderer>().material.color;

            if (c.g == 0 && c.b == 0)
            {
                if (c.r - 0.01f <= 0)
                {

                    GameObject.Destroy(cube);
                    placed_cubes.Remove(point);
                    return;
                }
                cube.GetComponent<Renderer>().material.color = new Color(c.r - 0.01f, c.g, c.b);
                print("Color" + cube.GetComponent<Renderer>().material.color);
            }
        }
    }

    void Clicked()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit = new RaycastHit();
        int layer_mask = LayerMask.GetMask("Floor");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {
            Vector3 point  = transform.InverseTransformPoint(hit.point);
            point.x = (step_x * Mathf.Floor(point.x / step_x) + step_x * Mathf.Ceil(point.x / step_x))/2;
            point.z = (step_z * Mathf.Floor(point.z / step_z) + step_z * Mathf.Ceil(point.z / step_z))/2;
            point.y = 0;
            if (placed_cubes.ContainsKey(point))
            {
                placed_cubes[point].GetComponent<Renderer>().material.color = Color.red;
                // Cambiar color
                return;
            }
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localPosition = point;
            Vector3 new_scale = new Vector3(floorCollider.bounds.size.x/rows, 1, floorCollider.bounds.size.z/columns);
            cube.transform.localScale = new_scale;
            cube.transform.parent = transform;
            placed_cubes.Add(point, cube);
        }
    }
}