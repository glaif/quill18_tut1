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
	
	// Update is called once per frame
	void Update () {

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (mouseRay.direction.y >= 0) {
            Debug.LogError("Why is mouse pointing up?");
            return;
        }
        // What point does mouseRay intersect with Y=0
        float rayLength = mouseRay.origin.y / mouseRay.direction.y;
        Vector3 hitPos = mouseRay.origin - (mouseRay.direction * rayLength);

        if (Input.GetMouseButtonDown(0)) {
            // Mouse butten went down, start dragging camera
            isDraggingCamera = true;
            lastMousePosition = hitPos;
        } else if (Input.GetMouseButtonUp(0)) {
            // Mouse butten went up, stop dragging camera
            isDraggingCamera = false;
        }

        if (isDraggingCamera) {
            Vector3 diff = lastMousePosition - hitPos;
            Camera.main.transform.Translate(diff, Space.World);

            mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (mouseRay.direction.y >= 0) {
                Debug.LogError("Why is mouse pointing up?");
                return;
            }
            // What point does mouseRay intersect with Y=0
            rayLength = mouseRay.origin.y / mouseRay.direction.y;
            lastMousePosition = hitPos = mouseRay.origin - (mouseRay.direction * rayLength);
        }

        // Zoom in/out on mouse wheel movement
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollAmount) > MIN_ZOOM_INCR) {
            // Move camera towards hitPos
            Vector3 dir = hitPos - Camera.main.transform.position;
            Vector3 p = Camera.main.transform.position;

            //Debug.LogError(scrollAmount);
            //Debug.LogError(p.y);
            // Stop zooming when you hit boundary
            if ((scrollAmount > 0) && (p.y > MIN_HEIGHT)) {
                // zooming in above floor
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            } else if ((scrollAmount < 0) && (p.y < MAX_HEIGHT)) {
                // zooming out below ceiling
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }

            p = Camera.main.transform.position;

            if (p.y < MIN_HEIGHT) {
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
                    (p.y-MIN_HEIGHT) / (MAX_HEIGHT / ROTATION_DAMPENER)
                ),
                Camera.main.transform.rotation.eulerAngles.y,
                Camera.main.transform.rotation.eulerAngles.z
            );
            //float angle = Camera.main.transform.rotation.eulerAngles.x;
            //Debug.LogError(angle);
        }
    }
}
