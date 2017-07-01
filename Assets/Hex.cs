using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public readonly HexMap HexMap;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;
    static readonly float RADIUS = 1f;

    HashSet<Unit> units;

    public Hex(HexMap hexMap, int q, int r) {
        this.HexMap = hexMap;
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

    public Vector3 PositionFromCamera() {
        return HexMap.GetHexPosition(this);
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
        if (HexMap.allowWrapEastWest) {
            int howManyWidthsToFix = Mathf.RoundToInt((position.x - cameraPosition.x) / mapWidth);
            position.x -= howManyWidthsToFix * mapWidth;
        }

        if (HexMap.allowWrapNorthSouth) {
            int howManyHeightsToFix = Mathf.RoundToInt((position.z - cameraPosition.z) / mapHeight);
            position.z -= howManyHeightsToFix * mapHeight;
        }

        return position;
    }

    public static float Distance(Hex a, Hex b) {
        int dQ = Mathf.Abs(a.Q - b.Q);
        if (a.HexMap.allowWrapEastWest) {
            if (dQ > a.HexMap.NumRows / 2)
                dQ = a.HexMap.NumRows - dQ;
        }

        int dR = Mathf.Abs(a.R - b.R);
        if (a.HexMap.allowWrapNorthSouth) {
            if (dR > a.HexMap.NumCols / 2)
                dR = a.HexMap.NumCols - dR;
        }

        return
            Mathf.Max(
                dQ,
                dR,
                Mathf.Abs(a.S - b.S)
            );
    }

    public void AddUnit(Unit unit) {
        if (units == null) {
            units = new HashSet<Unit>();
        }
        units.Add(unit);
    }

    public void RemoveUnit(Unit unit) {
        if (units != null) {
            units.Remove(unit);
        }
    }

    public Unit[] Units() {
        return units.ToArray();
    }
}
