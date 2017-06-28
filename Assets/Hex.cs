using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to define Hex object positions
/// Does not interact with Unity directly
/// </summary>
public class Hex {
    // Must hold in cubic coordinates: Q + R + S = 0

    public readonly int Q;  // column
    public readonly int R;  // row
    public readonly int S;  // sum

	public float Elevation;
	public float Moisture;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;
    static readonly float RADIUS = 1f;

	// TODO: Link with HexMap version of this
    bool allowWrapEastWest = true;
    bool allowWrapNorthSouth = false;

    public Hex(int q, int r) {
        this.Q = q;
        this.R = r;
        this.S = -(q + r);
    }

    public Vector3 Position() {
        return new Vector3(
            HexHorizontalSpacing() * (this.Q + (this.R * 0.5f)),
            0,
            HexVerticalSpacing() * this.R
        );
    }

    public float HexHeight() {
        return RADIUS * 2;
    }

    public float HexWidth() {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing() {
        return HexHeight() * 0.75f;
    }

    public float HexHorizontalSpacing() {
        return HexWidth();
    }

    public Vector3 PositionFromCamera(Vector3 cameraPosition, float numRows, float numCols) {
        float mapHeight = numRows * HexVerticalSpacing();
        float mapWidth = numCols * HexHorizontalSpacing();

        Vector3 position = Position();
        // GOAL: keep howManyWidthsFromCamera between -0.5 and 0.5;

        // If current Hex is 0.6 mapWidths away from camera, then it should be -0.4
        // 0.8 -> -0.2
        // 2.2 ->  0.2
        // 2.8 -> -0.2
        // 2.6 -> -0.4
        // -0.6 -> 0.4
        if (allowWrapEastWest) {
            int howManyWidthsToFix = Mathf.RoundToInt((position.x - cameraPosition.x) / mapWidth);
            position.x -= howManyWidthsToFix * mapWidth;
        }

        if (allowWrapNorthSouth) {
            int howManyHeightsToFix = Mathf.RoundToInt((position.z - cameraPosition.z) / mapHeight);
            position.z -= howManyHeightsToFix * mapHeight;
        }

        return position;
    }

}
