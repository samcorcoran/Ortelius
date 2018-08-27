using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputController : MonoBehaviour {

    VisualizeWorld WorldVisualizer;

    public float worldViewportSize = 0.75f;
    Camera worldCamera;
    Camera mainCamera;

    Vector3 cameraFocusPoint = Vector3.zero;

    float worldScale = 6.371f;
    float cameraDistanceFromWorldSurface = 10f;

    public float cameraLatitude = 0.1f;
    public float cameraLatitudeMaxStep = 7f;
    public float cameraCurrentLatitudeStep = 0f;
    public float cameraLatitudeStepAcceleration = 1.0f;
    public float cameraLatitudeStepDecay = 0.05f;
    public float cameraLatitudeMinStep = 0.01f;

    public float cameraLongitude = 0.1f;
    public float cameraLongitudeMaxStep = 10f;
    public float cameraCurrentLongitudeStep = 0f;
    public float cameraLongitudeStepAcceleration = 1.0f;
    public float cameraLongitudeStepDecay = 0.05f;
    public float cameraLongitudeMinStep = 0.01f;


    // Use this for initialization
    void Start () {
        WorldVisualizer = GetComponentInParent<VisualizeWorld>();
        mainCamera = Camera.main;
        foreach (var camera in Camera.allCameras)
        {
            if (camera.tag == "WorldCamera")
            {
                worldCamera = camera;
                break;
            }
        }
        worldCamera.transform.LookAt(cameraFocusPoint);
	}

    private void UpdateCameraRects()
    {
        worldViewportSize = Mathf.Max(worldViewportSize, 0);
        worldViewportSize = Mathf.Min(worldViewportSize, 1);
        worldCamera.rect = new Rect(0, 0, worldViewportSize, 1);
        mainCamera.rect = new Rect(worldViewportSize, 0, 1 - worldViewportSize, 1);
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyInput();
        HandleMouseInput();
        UpdateCameraRects();
    }

    private void FixedUpdate()
    {
        // Move camera to lon/lat position
        worldCamera.transform.position = GeographicToCartesian(cameraLongitude, cameraLatitude) * (worldScale + cameraDistanceFromWorldSurface);
        // Protect against flipping upside down
        worldCamera.transform.up = Vector3.up;
        // Turn back to look, one last time
        worldCamera.transform.LookAt(cameraFocusPoint);
    }

    private Vector3 GeographicToCartesian(float longitude, float latitude)
    {
        return new Vector3(Mathf.Cos(Mathf.Deg2Rad*latitude) * Mathf.Cos(Mathf.Deg2Rad*longitude),
                           Mathf.Sin(Mathf.Deg2Rad * latitude),
                           Mathf.Cos(Mathf.Deg2Rad*latitude) * Mathf.Sin(Mathf.Deg2Rad*longitude));
    }

    private void PrintCameraLocation()
    {
        Debug.Log("Camera at lon/lat: " + cameraLongitude.ToString() + ", " + cameraLatitude.ToString());
    }

    private void CentreCameraOnHome()
    {
        cameraLatitude = 0.0f;
        cameraCurrentLatitudeStep = 0f;
        cameraLongitude = 0.0f;
        cameraCurrentLongitudeStep = 0f;
    }

    private void HandleKeyInput()
    {
        // Longitude
        bool longitudeChanged = false;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            longitudeChanged = true;
            // Rotate camera clockwise (west)
            cameraLongitude -= cameraCurrentLongitudeStep;
            if (cameraLongitude < -180)
            {
                cameraLongitude += 360;
            }
            PrintCameraLocation();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            longitudeChanged = true;
            // Rotate camera anti-clockwise (east)
            cameraLongitude += cameraCurrentLongitudeStep;
            if (cameraLongitude > 180)
            {
                cameraLongitude -= 360;
            }
            PrintCameraLocation();
        }
        // Latitude
        bool latitudeChanged = false;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            latitudeChanged = true;
            // Lift the camera
            cameraLatitude += cameraCurrentLatitudeStep;
            cameraLatitude = Mathf.Min(cameraLatitude, 89f);
            PrintCameraLocation();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            latitudeChanged = true;
            // Lower the camera
            cameraLatitude -= cameraCurrentLatitudeStep;
            cameraLatitude = Mathf.Max(cameraLatitude, -89f);
            PrintCameraLocation();
        }

        // Update latitude step
        if (latitudeChanged)
        {
            // Increase latitude step
            cameraCurrentLatitudeStep = Mathf.Min(cameraCurrentLatitudeStep + cameraLatitudeStepAcceleration, cameraLatitudeMaxStep);
        }
        else
        {
            cameraCurrentLatitudeStep = Mathf.Max(cameraCurrentLatitudeStep - cameraLatitudeStepDecay, cameraLatitudeMinStep);
        }
        // Update longitude step
        if (longitudeChanged)
        {
            // Increase latitude step
            cameraCurrentLongitudeStep = Mathf.Min(cameraCurrentLongitudeStep + cameraLongitudeStepAcceleration, cameraLongitudeMaxStep);
        }
        else
        {
            cameraCurrentLongitudeStep = Mathf.Max(cameraCurrentLongitudeStep - cameraLongitudeStepDecay, cameraLongitudeMinStep);
        }

        // Jump to zero zero and kill movement
        if (Input.GetKeyDown(KeyCode.Home))
        {
            CentreCameraOnHome();
        }
    }

    public void HandleMouseInput()
    {
        // Get mouse screen position
        Ray placefinder = worldCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(placefinder, out hit, 300))
        {
            Debug.Log(hit.point);
            // Get Id of nearest cell
            var cell = WorldVisualizer.GetContainingCell(hit.point);
            Debug.Log("Cell:");
            Debug.Log(cell.Id.ToString());
            Debug.Log(cell.Position.Latitude.ToString() + ", " + cell.Position.Longitude.ToString());
            WorldVisualizer.HoverCell(cell);
            if (Input.GetMouseButtonDown(0))
            {
                WorldVisualizer.SelectCell(cell);
            }
        }
    }
}
