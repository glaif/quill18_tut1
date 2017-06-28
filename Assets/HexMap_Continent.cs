using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap {
	override public void generateMap() {
		// First call the base version to generate all the hexes we need
		base.generateMap ();

		ElevateArea (21, 15, 4);

		UpdateHexVisuals ();
	}

	void ElevateArea(int q, int r, int radius) {
		Hex centerHex = GetHexAt (q, r);

		centerHex.Elevation = 0.5f;
	}

}
