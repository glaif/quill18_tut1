using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap {
    override public void generateMap() {
        // First call the base version to generate all the hexes we need
        base.generateMap();

        //Debug.LogError("Calling ElevateArea");

        int numSplats = Random.Range(4, 8);
        for (int i = 0; i < numSplats; i++) {
            int range = Random.Range(3, 8);
            int y = Random.Range(range, NumRows - range);
            int x = Random.Range(0, 10) - y/2 + 20;

            ElevateArea(x, y, range);
        }
        UpdateHexVisuals();
    }

    void ElevateArea(int q, int r, int range, float centerHeight = 0.5f) {
        Hex centerHex = GetHexAt(q, r);

        //centerHex.Elevation = 0.5f;

        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);
        //Debug.LogError("In ElevateArea");
        foreach (Hex h in areaHexes) {
            if (h.Elevation < 0)
                h.Elevation = 0;

            //Debug.LogError(string.Format("HEX {0},{1} elevated", h.Q, h.R));
            h.Elevation += centerHeight * Mathf.Lerp(1f, 0.25f, Hex.Distance(centerHex, h) / range);
        }
    }
}
