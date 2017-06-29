using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
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
        if (Mathf.Abs(scrollAmount) > 0.01f) {
            // Move camera towards hitPos
            Vector3 dir = hitPos - Camera.main.transform.position;
            Vector3 p = Camera.main.transform.position;

            //Debug.LogError(scrollAmount);
            //Debug.LogError(p.y);
            // Stop zooming when you hit boundary
            if ((scrollAmount > 0) && (p.y > 4f)) {
                // zooming in above floor
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            } else if ((scrollAmount < 0) && (p.y < 20f)) {
                // zooming out below ceiling
                Camera.main.transform.Translate(dir * scrollAmount, Space.World);
            }

            p = Camera.main.transform.position;

            if (p.y < 4f) {
                p.y = 4f;
            } else if (p.y > 20f) {
                p.y = 20f;
            }
            Camera.main.transform.position = p;
        }
    }
}
