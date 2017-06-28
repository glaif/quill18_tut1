using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap {
    override public void generateMap() {
        // First call the base version to generate all the hexes we need
        base.generateMap();

        // Make some kind of raised area
        int numContinents = 3;
        int continentSpacing = NumCols / numContinents;

        Random.InitState(0);
        for (int c = 0; c < numContinents; c++) {
            int numSplats = Random.Range(4, 8);
            for (int i = 0; i < numSplats; i++) {
                int range = Random.Range(5, 8);
                int y = Random.Range(range, NumRows - range);
                int x = Random.Range(0, 10) - y/2 + (c * continentSpacing);

                ElevateArea(x, y, range);
            }
        }

        // Add topology using Perlin Noise
        float noiseResolution = 0.01f;
        Vector2 noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        float noiseScale = 2f; // larger values makes more islands and lakes

        //float vertScaleFixer = 2f; // fix because map is rect but Perlin map is square

        for (int column = 0; column < NumCols; column++) {
            for (int row = 0; row < NumRows; row++) {
                Hex h = GetHexAt(column, row);
                float n = Mathf.PerlinNoise(
                    ((float)column/Mathf.Max(NumCols, NumRows) / noiseResolution) + noiseOffset.x, 
                    ((float)row/Mathf.Max(NumCols, NumRows) / noiseResolution) + noiseOffset.y) 
                    - 0.5f;
                h.Elevation += n * noiseScale;
            }
        }

        // Add moisture using Perlin Noise
        noiseResolution = 0.05f;
        noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        noiseScale = 2f; // larger values makes more islands and lakes

        //float vertScaleFixer = 2f; // fix because map is rect but Perlin map is square

        for (int column = 0; column < NumCols; column++) {
            for (int row = 0; row < NumRows; row++) {
                Hex h = GetHexAt(column, row);
                float n = Mathf.PerlinNoise(
                    ((float)column / Mathf.Max(NumCols, NumRows) / noiseResolution) + noiseOffset.x,
                    ((float)row / Mathf.Max(NumCols, NumRows) / noiseResolution) + noiseOffset.y)
                    - 0.5f;
                h.Moisture += n * noiseScale;
            }
        }

        UpdateHexVisuals();
    }

    void ElevateArea(int q, int r, int range, float centerHeight = 0.8f) {
        Hex centerHex = GetHexAt(q, r);

        //centerHex.Elevation = 0.5f;

        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);
        //Debug.LogError("In ElevateArea");
        foreach (Hex h in areaHexes) {
            if (h.Elevation < 0)
                h.Elevation = 0;

            //Debug.LogError(string.Format("HEX {0},{1} elevated", h.Q, h.R));
            h.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(Hex.Distance(centerHex, h) / range, 2f));
        }
    }
}
