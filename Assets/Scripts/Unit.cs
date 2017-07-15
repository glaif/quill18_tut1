using QPath;
using System.Collections.Generic;
using UnityEngine;

public class Unit : IQPathUnit {

    public string Name = "Dwarf";
    public int HitPoints = 100;
    public int Strength = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

    public Hex Hex { get; protected set; }

    public delegate void UnitMovedDelegate(Hex oldHex, Hex newHex);
    public event UnitMovedDelegate OnUnitMoved;

    /// <summary>
    /// List of hexes to walkthrough (from pathfinder).
    /// NOTE: The first item is always the hex we are standing in.
    /// </summary>
    List<Hex> hexPath;

    // TODO: MOve to central option/config file
    const bool MOVEMENT_RULES_LIKE_CIV6 = false;

    public void SetHex(Hex newHex) {

        Hex oldHex = Hex;

        if (Hex != null) {
            Hex.RemoveUnit(this);
        }
        Hex = newHex;
        Hex.AddUnit(this);

        if (OnUnitMoved != null) {
            OnUnitMoved(oldHex, newHex);
        }
    }

    public void DUMMY_PATHING_FUNCTION() {
        Hex[] pathHexes = QPath.QPath.FindPath<Hex>(
            Hex.HexMap,
            this,
            Hex,
            Hex.HexMap.GetHexAt(Hex.Q + 6, Hex.R),
            Hex.CostEstimate
         );

        Debug.LogError("Got pathfinding path of length: " + pathHexes.Length);

        SetHexPath(pathHexes);
    }

    public void ClearHexPath() {
        this.hexPath = new List<Hex>();
    }

    public void SetHexPath(Hex[] hexArray) {
        this.hexPath = new List<Hex>(hexArray);
    }

    public Hex[] GetHexPath() {
        return (this.hexPath == null) ? null : this.hexPath.ToArray();
    }

    public bool UnitWaitingForOrders() {
        // Returns true if we have movement left but nothing queued
        // TODO: maybe we've been told to fortify / alert / skip turn
        if (MovementRemaining > 0 && (hexPath == null || hexPath.Count==0)) {
            return true;
        }
        return false;
    }

    public void RefreshMovement() {
        MovementRemaining = Movement;
    }

    /// <summary>
    /// Processes one tile's worth of movement for the unit
    /// </summary>
    /// <returns>Returns true of this should be called immediately again</returns>
    public bool DoMove() {
        // Do queued move

        if (MovementRemaining <= 0)
            return false;

        if (hexPath == null || hexPath.Count == 0) {
            return false;
        }

        // Grab the first Hex from the queue
        // Remove the hex we are leaving first
        Hex hexWeAreLeaving = hexPath[0];
        Hex newHex = hexPath[1];

        int costToEnter = MovementCostToEnterHex(newHex);

        if ((costToEnter > MovementRemaining) && 
            (MovementRemaining < Movement) && 
            MOVEMENT_RULES_LIKE_CIV6) {

            // We can't enter the hex this turn
            return false;
        }

        hexPath.RemoveAt(0);

        if (hexPath.Count == 1) {
            // The only hex in the queue is the last one in our
            // path, so clear the hexPath.
            hexPath = null;
        }

        // Move to the new hex
        SetHex(newHex);

        MovementRemaining = Mathf.Max(MovementRemaining - costToEnter, 0);

        return (hexPath != null && MovementRemaining > 0);
    }

    public int MovementCostToEnterHex(Hex hex) {
        // TODO: implement different movement traits

        return hex.BaseMovementCost(false, false, false);
    }

    public float AggregateTurnsToEnterHex(Hex hex, float turnsToDate) {
        // A unit may want to enter a tile with a cost greater than  the
        // remaining movement budget.  This can be handled in two ways.
        // You can return a lower-than-expected cost (e.g., Civ5) or
        // a higher-than-expected turn cost (e.g., Civ6).

        float baseTurnsToEnterHex = MovementCostToEnterHex(hex) / Movement;

        if (baseTurnsToEnterHex < 0) {
            // Impassable terrain
            //Debug.LogError("Impassable terrain: Hex " + hex.toString());
            return -99999;
        }

        if (baseTurnsToEnterHex > 1) {
            // Even if something costs 3 to enter and we have a max move of 2,
            // you can always enter if using a full turn of movement.
            baseTurnsToEnterHex = 1;
        }

        float turnsRemaining = MovementRemaining / Movement;

        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;
        float turnsUsedAfterThisMove = 0;

        if ((turnsToDateFraction > 0f && turnsToDateFraction < 0.01f) || turnsToDateFraction > 0.99f) {
            Debug.Log("Looks like we have floating point drift: " + turnsToDate);

            if (turnsToDateFraction < 0.01f)
                turnsToDateFraction = 0f;

            if (turnsToDateFraction > 0.99f) {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }

            turnsUsedAfterThisMove = turnsToDateFraction + baseTurnsToEnterHex;

            if (turnsUsedAfterThisMove > 1) {
                // This is the where we don't have enough movement to complete this move
                if (MOVEMENT_RULES_LIKE_CIV6) {
                    // We aren't allowed to enter the time this move
                    if (turnsToDateFraction == 0) {
                        // We have full movement (fresh turn), but this isn't enough to enter the tile
                        // E.g., max move of 2, but the tile costs 3 to enter
                        // We are good to go.
                    } else {
                        // We are NOT on a fresh turn -- therefore we need to
                        // Sit idle for the remainder of this turn
                        turnsToDateWhole += 1;
                        turnsToDateFraction = 0;
                    }

                    // So now we know for certain that we are starting the move into difficult terrain 
                    // on a fresh turn.
                    turnsUsedAfterThisMove = baseTurnsToEnterHex;
                } else {
                    // Civ5-style movement state that we can always enter a tile, even if we don't
                    // have enough movement left.
                    turnsUsedAfterThisMove = 1;
                }
            }
        }
        // turnsUsedAfterThisMove i snow some value from 0..1 (this includes
        // the factional part of the move from previous turns).

        // This function will return the total turn cost of 
        // turnsToDate + turns for this move.

        return turnsToDateWhole + turnsUsedAfterThisMove;
    }

    /// <summary>
    /// Turn cost to enter a hex (e.g., 0.5 turns if a movement cost is 1 and we have 2 max movement)
    /// </summary>
    public float CostToEnterHex(IQPathTile sourceTile, IQPathTile destinationTile) {
        return 1;
    }
}