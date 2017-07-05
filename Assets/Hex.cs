using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QPath;
using System;

/// <summary>
/// Helper class to define Hex object positions
/// Does not interact with Unity directly
/// </summary>
public class Hex : IQPathTile {
    // Must hold in cubic coordinates: Q + R + S = 0

    public readonly int Q;  // column
    public readonly int R;  // row
    public readonly int S;  // sum

    public float Elevation;
    public float Moisture;

    // TODO: This is just a temporarily public value
    public int MovementCost = 1;

    // TODO: Need property to track Hex type (plains, grasslands, etc.)
    // TODO: Need property to track Hex details (fores, mine, farm, etc.)

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

    public static float CostEstimate(IQPathTile aa, IQPathTile bb) {
        return Distance((Hex) aa, (Hex) bb);
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

    public int BaseMovementCost() {
        // TODO: Factor in terrain type and features
        return this.MovementCost;
    }

    Hex[] neighbours;

    public IQPathTile[] GetNeighbours() {
        if (this.neighbours != null)
            return this.neighbours;
        List<Hex> neighbours = new List<Hex>();
        List<Hex> neighbours2 = new List<Hex>();

        //for (int q = -1; q < 2; q++) {
        //    for (int r = -1; r < 2; r++) {
        //        if ((q == 0 && r == 0) || (q == 1 && r == 1) || (q == -1 && r == -1)) {
        //            continue;
        //        }
        //        neighbours.Add(HexMap.GetHexAt(Q + q, R + r));
        //        //Debug.LogError("q: " + q + ", r: " + r);
        //    }
        //}
        neighbours.Add(HexMap.GetHexAt(Q + 1, R + 0));
        neighbours.Add(HexMap.GetHexAt(Q - 1, R + 0));
        neighbours.Add(HexMap.GetHexAt(Q + 0, R + 1));
        neighbours.Add(HexMap.GetHexAt(Q + 0, R - 1));
        neighbours.Add(HexMap.GetHexAt(Q + 1, R - 1));
        neighbours.Add(HexMap.GetHexAt(Q - 1, R + 1));

        foreach (Hex h in neighbours) {
            if (h != null) {
                neighbours2.Add(h);
            }
        }
        this.neighbours = neighbours2.ToArray();
        return this.neighbours;
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit theUnit) {
        // TODO: We ignmore the sourceTile right now, will change when we have rivers

        return ((Unit)theUnit).AggregateTurnsToEnterHex(this, costSoFar);
    }

    public String toString() {
        return Q + ", " +R;
    }
}
