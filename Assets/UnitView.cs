using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour {

    Vector3 newPosition;

    Vector3 currentVelocity;
    float smoothTime = 0.5f;

    private void Start() {
        newPosition = this.transform.position;
    }

    // Use this for initialization
    public void OnUnitMoved(Hex oldHex, Hex newHex) {
        // Animate the unit moving from oldHex to newHex

        this.transform.position = oldHex.PositionFromCamera();
        newPosition = newHex.PositionFromCamera();
        currentVelocity = Vector3.zero;

        if (Vector3.Distance(this.transform.position, newPosition) > 2) {
            // Large move, probably due to a map seam
            this.transform.position = newPosition;
        }
    }

    void Update() {
        this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
    }
}
