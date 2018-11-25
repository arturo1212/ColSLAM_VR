using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    public GameObject laserPrefab, laserWatchPrefab, watcherControl;
    private GameObject laser_teleport, laser_watch;

    public Vector3 teleportReticleOffset;
    private Vector3 hitPoint;

    public Transform cameraRigTransform;
    public Transform headTransform;
    private Transform watchedTransform, watchHitTransform;
    private Transform laserTransform;

    public LayerMask teleportMask, watchMask;

    private bool shouldTeleport, shouldWatch, watching;

    public Vector2 touchPad;

    public ControllerData controllerData;

    // Use this for initialization
    void Start () {
        controllerData = GetComponent<ControllerData>();
        laser_teleport = Instantiate(laserPrefab);
        laser_watch = Instantiate(laserWatchPrefab);
    }

    // Update is called once per frame
    void Update () {
        if (controllerData.touchPadPressed)
        {
            RaycastHit hit;

            if (Physics.Raycast(controllerData.trackedObj.transform.position, transform.forward, out hit, 100, teleportMask))
            {
                hitPoint = hit.point;
                ShowLaser(hit, laser_teleport);
                shouldTeleport = true;
            }
        }
        else
        {
            laser_teleport.SetActive(false);
        }
        if (controllerData.triggerPressed)
        {
            RaycastHit hit;

            if (Physics.Raycast(controllerData.trackedObj.transform.position, transform.forward, out hit, 100, watchMask)) // Floor
            {
                hitPoint = hit.point;
                watchHitTransform = hit.transform;
                ShowLaser(hit, laser_watch);
                shouldWatch= true;
            }
        }
        else
        {
            laser_watch.SetActive(false);
        }

        if (controllerData.touchPadPressUp && shouldTeleport)
        {
            Teleport();
            watching = false;
            cameraRigTransform.localScale = new Vector3(6, 6, 6);
        }
        if(controllerData.triggerPressUp && shouldWatch)
        {
            Watch(watchHitTransform);
            watching = true;
        }
        if (watching)
        {
            Debug.Log("WATCHING");
            cameraRigTransform.position = watchedTransform.position;
        }
        else
        {
            Debug.Log("YOU ARE FREE");
        }
    }

    private void Watch(Transform plane)
    {
        shouldWatch = false;
        watchedTransform = plane.Find("Robot");
        cameraRigTransform.localScale = new Vector3(1, 1, 1)*0.25f;
        watcherControl.GetComponent<StreamScreen>().activeRobot = plane.Find("Robot").gameObject;
    }

    private void Teleport()
    {
        shouldTeleport = false;
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        difference.y = 0;
        cameraRigTransform.position = hitPoint + difference;
    }

    private void ShowLaser(RaycastHit hit, GameObject laser)
    {
        laser.SetActive(true);
        laserTransform = laser.transform;
        laserTransform.position = Vector3.Lerp(controllerData.trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }
}
