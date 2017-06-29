using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseController : MonoBehaviour {

    static readonly float MIN_HEIGHT = 4f;
    static readonly float MAX_HEIGHT = 20f;
    static readonly float MIN_ZOOM_INCR = 0.01f;
    static readonly float LOW_ROTATE_ANGLE = 40f;
    static readonly float HIGH_ROTATE_ANGLE = 90f;
    static readonly float ROTATION_DAMPENER = 1.5f;
    static readonly int   MOUSE_DRAG_THRESHOLD = 1; // Threshold of mouse movement to start a drag

    // Use this for initialization
    void Start () {
        // change camera angle
        Vector3 p = Camera.main.transform.position;

        Camera.main.transform.rotation = Quaternion.Euler(
            Mathf.Lerp(
                LOW_ROTATE_ANGLE,
                HIGH_ROTATE_ANGLE,
                (p.y - MIN_HEIGHT) / (MAX_HEIGHT / ROTATION_DAMPENER)
            ),
            Camera.main.transform.rotation.eulerAngles.y,
            Camera.main.transform.rotation.eulerAngles.z
        );
    }

    bool isDraggingCamera = false;
    Vector3 lastMousePosition;
    Vector3 lastMouseGroundPlanePosition;

    // Update is called once per frame
    void Update() {
        Update_CameraDrag();
        Update_ScrollZoom();
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

    void Update_CameraDrag() {
        if (Input.GetMouseButtonDown(0)) {
            // Mouse butten went down, start dragging camera
            isDraggingCamera = true;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
        } else if (Input.GetMouseButtonUp(0)) {
            // Mouse butten went up, stop dragging camera
            isDraggingCamera = false;
            return;
        } else if (Input.GetMouseButton(0) && Vector3.Distance(Input.mousePosition, lastMousePosition) > MOUSE_DRAG_THRESHOLD) {
            // Holding down left mouse button and moving
            Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
            Vector3 diff = lastMouseGroundPlanePosition - hitPos;
            Camera.main.transform.Translate(diff, Space.World);

            lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);
        }
    }

    void Update_ScrollZoom() {
        // Zoom in/out on mouse wheel movement
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollAmount) > MIN_ZOOM_INCR) {
            // Move camera towards hitPos
            Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
            Vector3 dir = hitPos - Camera.main.transform.position;
            Vector3 p = Camera.main.transform.position;

            //Debug.LogError(scrollAmount);
            //Debug.LogError(p.y);
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
            //Camera.main.transform.rotation = Quaternion.Euler(
            //    Mathf.Lerp(
            //        LOW_ROTATE_ANGLE, 
            //        HIGH_ROTATE_ANGLE, 
            //        (p.y-MIN_HEIGHT) / (MAX_HEIGHT / ROTATION_DAMPENER)
            //    ),
            //    Camera.main.transform.rotation.eulerAngles.y,
            //    Camera.main.transform.rotation.eulerAngles.z
            //);
            //float angle = Camera.main.transform.rotation.eulerAngles.x;
            //Debug.LogError(angle);
        }
    }
}
