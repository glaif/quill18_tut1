using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    static readonly float MIN_HEIGHT = 4f;
    static readonly float MAX_HEIGHT = 20f;
    static readonly float MIN_ZOOM_INCR = 0.01f;
    static readonly float LOW_ROTATE_ANGLE = 40f;
    static readonly float HIGH_ROTATE_ANGLE = 90f;
    static readonly float ROTATION_DAMPENER = 1.5f;
    static readonly int MOUSE_DRAG_THRESHOLD = 1; // Threshold of mouse movement to start a drag

    public LayerMask LayerIDForHexTiles;

    // Use this for initialization
    void Start() {
        Update_CurrentFunc = Update_DetectModeStart;

        hexMap = GameObject.FindObjectOfType<HexMap>();

        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
    }

    public GameObject UnitSelectionPanel;

    // Generic bookkeeping vars
    HexMap hexMap;
    Hex hexUnderMouse;
    Hex hexLastUnderMouse;
    Vector3 lastMousePosition;  // From Input.mousePosition

    // Camera dragging bookeeping vars
    Vector3 lastMouseGroundPlanePosition;  // Camera ray Y-plane intersetion point

    // Unit movement
    Unit __selectedUnit = null;
    public Unit SelectedUnit {
        get { return __selectedUnit; }
        set {
            __selectedUnit = value;
            UnitSelectionPanel.SetActive(__selectedUnit != null);
        }
    }

    Hex[] hexPath;
    LineRenderer lineRenderer;

    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;

    // Update is called once per frame
    void Update() {
        hexUnderMouse = MouseToHex();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            SelectedUnit = null;
            CancelUpdateFunc();
        }
        Update_CurrentFunc();

        // Always do camera zoom, check for a scrolling UI later
        Update_ScrollZoom();

        lastMousePosition = Input.mousePosition;
        hexLastUnderMouse = hexUnderMouse;

        if (SelectedUnit != null) {
            DrawPath((hexPath != null) ? hexPath : SelectedUnit.GetHexPath());
        } else {
            DrawPath(null); // Clear the path display
        }
    }

    void DrawPath(Hex[] hexPath) {
        if (hexPath == null || hexPath.Length == 0) {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;

        Vector3[] ps = new Vector3[hexPath.Length];

        for (int i = 0; i < hexPath.Length; i++) {
            GameObject hexGO = hexMap.GetHexGO(hexPath[i]);
            ps[i] = hexGO.transform.position + (Vector3.up * 0.1f);
        }
        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
    }

    void CancelUpdateFunc() {
        Update_CurrentFunc = Update_DetectModeStart;

        // Also do cleanup of any UI stuff associated with modes
        hexPath = null;
    }

    void Update_DetectModeStart() {
        if (Input.GetMouseButtonDown(0)) {
            // Left mouse button went down
            // This doesn't do anything by itself
            Debug.Log("MOUSE DOWN");

        } else if (Input.GetMouseButtonUp(0)) {
            // TODO: Are we clicking on a hex with a unit?
            //      If so, select it
            Debug.Log("MOUSE UP -- click!");

            Unit[] us = hexUnderMouse.Units();

            // TODO: implement cycling through multiple units in the same tile

            if (us.Length > 0) {
                SelectedUnit = us[0];

                // NOTE: selecting a unit does not change our mouse mode

                //Update_CurrentFunc = Update_UnitMovement;
            }
        } else if (SelectedUnit != null && Input.GetMouseButtonDown(1)) {
            // We have a selected unit, and we've pushed the down the right
            // mouse button, so enter unit move mode.

            Update_CurrentFunc = Update_UnitMovement;

        } else if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, lastMousePosition) > MOUSE_DRAG_THRESHOLD) {
            // Left button is being held down and the mouse moved
            // Camera drag
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();

        } else if (SelectedUnit != null && Input.GetMouseButton(1)) {
            // TODO: We have a selected unit, and we are holding down the mouse
            // button.  We are in unit movement mode -- show a path from 
            // unit to mouse position via the pathfinding system.
            Debug.Log("MOUSE DOWN -- movement mode");
        }
    }

    Hex MouseToHex() {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        int layerMask = LayerIDForHexTiles.value;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, layerMask)) {
            //Something got hit
            //Debug.Log(hitInfo.collider.name);

            GameObject hexGO = hitInfo.rigidbody.gameObject;

            return hexMap.GetHexFromGameObject(hexGO);
        }
        Debug.Log("MouseController::MouseToHex -- Found nothing!");

        return null;
    }

    Vector3 MouseToGroundPlane(Vector3 mousePos) {
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);

        if (mouseRay.direction.y >= 0) {
            Debug.LogError("Why is mouse pointing up?");
            return Vector3.zero;
        }
        // What point does mouseRay intersect with Y=0
        float rayLength = mouseRay.origin.y / mouseRay.direction.y;
        return (mouseRay.origin - (mouseRay.direction * rayLength));
    }

    void Update_UnitMovement() {
        if (Input.GetMouseButtonUp(1) || SelectedUnit == null) {
            // Mouse button went up, complete unit movement
            Debug.Log("Complete unit movement.");

            // Copy pathfinding path to unit's movement queue
            if (SelectedUnit != null) {
                SelectedUnit.SetHexPath(hexPath);

                // TODO: Tell unit and/or hexmap to process unit movement
                StartCoroutine(hexMap.DoUnitMoves(SelectedUnit));
            }

            CancelUpdateFunc();
            return;
        }

        // We have a selected unit

        // Look at the hex under our mouse
        // Is this a different hex than before
        if (hexPath == null || hexUnderMouse != hexLastUnderMouse) {
            // Do a pathfinding search to that hex
            hexPath = QPath.QPath.FindPath<Hex>(hexMap, SelectedUnit, SelectedUnit.Hex, hexUnderMouse, Hex.CostEstimate);
        }

    }

    void Update_CameraDrag() {
        if (Input.GetMouseButtonUp(0)) {
            // Mouse butten went up, stop dragging camera
            Debug.Log("Cancel camera drag.");

            CancelUpdateFunc();
            return;
        }
        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);
    }

    void Update_ScrollZoom() {
        // Zoom in/out on mouse wheel movement
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollAmount) > MIN_ZOOM_INCR) {
            // Move camera towards hitPos
            Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
            Vector3 dir = hitPos - Camera.main.transform.position;
            Vector3 p = Camera.main.transform.position;

            // Stop zooming when you hit boundary
            if ((scrollAmount > 0) && (p.y > MIN_HEIGHT)) {
                // zooming in above floor
                Camera.main.transform.Translate(dir* scrollAmount, Space.World);
            } else if ((scrollAmount< 0) && (p.y<MAX_HEIGHT)) {
                // zooming out below ceiling
                Camera.main.transform.Translate(dir* scrollAmount, Space.World);
            }

            p = Camera.main.transform.position;

            if (p.y<MIN_HEIGHT) {
                p.y = MIN_HEIGHT;
            } else if (p.y > MAX_HEIGHT) {
                p.y = MAX_HEIGHT;
            }
            Camera.main.transform.position = p;

            // change camera angle
            Camera.main.transform.rotation = Quaternion.Euler(
                Mathf.Lerp(
                    LOW_ROTATE_ANGLE,
                    HIGH_ROTATE_ANGLE,
                    (Camera.main.transform.position.y / MAX_HEIGHT)
                ),
                Camera.main.transform.rotation.eulerAngles.y,
                Camera.main.transform.rotation.eulerAngles.z
            );
        }
    }
}
