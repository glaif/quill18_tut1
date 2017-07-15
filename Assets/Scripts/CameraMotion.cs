using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}


    Vector3 oldPosition;

    // Update is called once per frame
	void Update () {

        // TODO: Code to click-and-drag camera
        //      WASD
        //      Zoom in and out

        CheckIfCameraMoved();
		
	}

    public void PanToHex( Hex hex) {
        // TODO: Move camera to hex
    }

    HexComponent[] hexes;

    void CheckIfCameraMoved() {
        if (oldPosition != this.transform.position) {
            // SOMETHING moved the camera
            oldPosition = this.transform.position;

            // TODO: Probably HexMap will have a dict of all these later
            if (hexes == null)
                hexes = GameObject.FindObjectsOfType<HexComponent>();

            // TODO: Maybe find a better way to decide which Hexes to update

            foreach (HexComponent hex in hexes) {
                hex.UpdatePosition();
            }
        }
    }
}
