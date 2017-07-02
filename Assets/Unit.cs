﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit {

    public string Name = "Dwarf";
    public int HitPoints = 100;
    public int Strength = 8;
    public int Movement = 2;
    public int MovementRemaining = 2;

    public Hex Hex { get; protected set; }

    public delegate void UnitMovedDelegate(Hex oldHex, Hex newHex);
    public event UnitMovedDelegate OnUnitMoved;

    Queue<Hex> hexPath;

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

    public void SetHexPath(Hex[] hexPath) {
        this.hexPath = new Queue<Hex>(hexPath);
    }

    public void DoTurn() {
        // Do queued move
        if (hexPath == null || hexPath.Count == 0) {
            return;
        }

        // Grab the first Hex from the queue
        Hex newHex = hexPath.Dequeue();

        // Move to the new hex
        SetHex(newHex);
    }

    public int MovementCostToEnterHex(Hex hex) {
        // TODO: override base movement cost based 
        // on unit movement mode + tile type
        return hex.BaseMovementCost();
    }

    public float AggregateTurnsToEnterHex(Hex hex, float turnsToDate) {
        // A unit may want to enter a tile with a cost greater than  the
        // remaining movement budget.  This can be handled in two ways.
        // You can return a lower-than-expected cost (e.g., Civ5) or
        // a higher-than-expected turn cost (e.g., Civ6).

        float baseTurnsToEnterHex = MovementCostToEnterHex(hex) / Movement;
        if (baseTurnsToEnterHex > 1) {
            // Even if something costs 3 to enter and we have a max move of 2,
            // you can always enter if using a full turn of movement.
            baseTurnsToEnterHex = 1;
        }

        float turnsRemaining = MovementRemaining / Movement;

        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;
        float turnsUsedAfterThisMove = 0;

        if (turnsToDateFraction < 0.01f || turnsToDateFraction > 0.99f) {
            Debug.Log("Looks like we have floating point drift.");

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
}