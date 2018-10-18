using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {

    public float updateInterval = 0.5F;
    public GameObject Obstacle;
    public List<GameObject> markers = new List<GameObject>();
    public int rows = 10, columns = 10, cloud_min_count = 3; // UTILIZAR VALORES PARES

    double lastInterval;
    int current_marker = 0;
    Dictionary<Vector3, GameObject> placed_cubes = new Dictionary<Vector3, GameObject>();
    Dictionary<Vector3, int> point_count = new Dictionary<Vector3, int>(); 
    Collider floorCollider;
    float step_x, step_z, start_x, start_z;

    public void createGrid()
    {
        floorCollider = gameObject.GetComponent(typeof(Collider)) as Collider;
        //Vector3 foorSize = new Vector3(floorCollider.bounds.size.x, floorCollider.bounds.size.z);
        print(floorCollider.bounds.center);
        //print(foorSize);

        step_x = floorCollider.bounds.size.x / rows;
        step_z = floorCollider.bounds.size.z / columns;

        start_x = floorCollider.bounds.center.x - floorCollider.bounds.size.x / 2;
        start_z = floorCollider.bounds.center.z - floorCollider.bounds.size.z / 2;

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
            lineRenderer.SetPosition(0, new Vector3(floorCollider.bounds.center.x - floorCollider.bounds.size.x / 2, 0.01f, i));
            lineRenderer.SetPosition(1, new Vector3(floorCollider.bounds.center.x + floorCollider.bounds.size.x / 2, 0.01f, i));
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
            ObstacleFound();
        }
        if (Input.GetMouseButtonDown(1))
        {
            MarkerFound();
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

            if (true)
            {
                if (c.r - 0.01f >= 1)
                {
                    GameObject.Destroy(cube);
                    placed_cubes.Remove(point);
                    point_count.Remove(point);
                    return;
                }
                cube.GetComponent<Renderer>().material.color = new Color(c.r + 0.01f, c.g + 0.01f, c.b + 0.01f, c.a - 0.01f);
                print("Color" + cube.GetComponent<Renderer>().material.color);
            }
        }
    }

    // Extender para especificar color y timeout
    public GameObject createCube(Vector3 point)
    {
        GameObject cube = Instantiate(Obstacle);
        point.x = (step_x * Mathf.Floor(point.x / step_x) + step_x * Mathf.Ceil(point.x / step_x)) / 2;
        point.z = (step_z * Mathf.Floor(point.z / step_z) + step_z * Mathf.Ceil(point.z / step_z)) / 2;
        point.y = 0;
        cube.transform.localPosition = point;
        Vector3 new_scale = new Vector3(floorCollider.bounds.size.x / rows, 1, floorCollider.bounds.size.z / columns);
        cube.transform.localScale = new_scale;
        cube.transform.parent = transform;
        return cube;
    }

    #region DEBUG cubos a mano
    Vector3? get_clicked_center()
    {
        /* Obtener punto seleccionado */
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        int layer_mask = LayerMask.GetMask("Floor");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask))
        {
            if (hit.collider.gameObject != this.gameObject)
            {
                return null;
            }
            //Vector3 point  = transform.InverseTransformPoint(hit.point);
            Vector3 point = hit.point;
            point.x = (step_x * Mathf.Floor(point.x / step_x) + step_x * Mathf.Ceil(point.x / step_x)) / 2;
            point.z = (step_z * Mathf.Floor(point.z / step_z) + step_z * Mathf.Ceil(point.z / step_z)) / 2;
            point.y = 0;
            return point;
        }
        return null;
        //return point;
    }

    public void ObstacleFound()
    {
        /* Obtener punto seleccionado */
        var clicked_point = get_clicked_center();
        if (clicked_point == null)
        {
            return;
        }
        Vector3 point = (Vector3)clicked_point;


        if (point_count.ContainsKey(point))
        {
            point_count[point] += 1;
            if (point_count[point] >= cloud_min_count)
            {
                if (!placed_cubes.ContainsKey(point))
                {
                    GameObject cube = createCube(point);
                    cube.GetComponent<Renderer>().material.color = Color.black;
                    placed_cubes.Add(point, cube);
                }
                else
                {
                    GameObject cube = placed_cubes[point];
                    cube.GetComponent<Renderer>().material.color = Color.black;
                }
            }
            return;
        }
        point_count[point] = 1;
    }

    void MarkerFound()
    {
        var clicked_point = get_clicked_center();
        if(clicked_point == null)
        {
            return;
        }
        GameObject marker = Instantiate(markers[current_marker]);
        marker.transform.localPosition = (Vector3)clicked_point;
        Vector3 new_scale = new Vector3(floorCollider.bounds.size.x / rows, 1, floorCollider.bounds.size.z / columns);
        marker.transform.localScale = new_scale;
        marker.transform.parent = transform;
        current_marker++;
        if(current_marker >= markers.Count)
        {
            current_marker = 0;
        }
    }
    #endregion


}
