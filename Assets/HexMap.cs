using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		generateMap ();
	}

	public GameObject hexPrefab;

	public Mesh MeshWater;
	public Mesh MeshFlat;
	public Mesh MeshHill;
	public Mesh MeshMountain;

	public Material MatOcean;
	public Material MatPlains;
	public Material MatGrasslands;
	public Material MatMountains;

	public readonly int NumRows = 30;
	public readonly int NumCols = 60;

	// TODO: Link with Hex version of this
	bool allowWrapEastWest = true;
	bool allowWrapNorthSouth = false;

	private Hex[,] hexes;
	private Dictionary<Hex, GameObject> hexToGameObjectMap;

	public Hex GetHexAt (int x, int y)
	{
		if (hexes == null) {
			Debug.LogError ("Hexes array not yet instantiated!");
			return null;	
		}

		if (allowWrapEastWest)
			x = x % NumCols;
		
		if (allowWrapNorthSouth)
			y = y % NumRows;
		
		return hexes [x, y];
	}

	virtual public void generateMap ()
	{
		// Generate a map filled with ocean

		hexes = new Hex[NumCols, NumRows];
		hexToGameObjectMap = new Dictionary<Hex, GameObject> ();

		for (int col = 0; col < NumCols; col++) {
			for (int row = 0; row < NumRows; row++) {

				Hex h = new Hex (col, row);

				hexes [col, row] = h;
				h.Elevation = -1;

				Vector3 pos = h.PositionFromCamera (
					                          Camera.main.transform.position,
					                          NumRows,
					                          NumCols
				                          );

				GameObject hexGO = Instantiate (
					                   hexPrefab, 
					                   pos, 
					                   Quaternion.identity, 
					                   this.transform);
				
				hexToGameObjectMap [h] = hexGO;

				hexGO.name = string.Format ("HEX: {0},{1}", col, row);
				hexGO.GetComponent<HexComponent> ().Hex = h;
				hexGO.GetComponent<HexComponent> ().HexMap = this;

				hexGO.GetComponentInChildren<TextMesh> ().text = string.Format ("{0},{1}", col, row);


			}
		}

		//StaticBatchingUtility.Combine(this.gameObject);
		UpdateHexVisuals ();
	}

	public void UpdateHexVisuals ()
	{
		for (int col = 0; col < NumCols; col++) {
			for (int row = 0; row < NumRows; row++) {
				Hex h = hexes [col, row];
				GameObject hexGO = hexToGameObjectMap [h];

				MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer> ();
				if (h.Elevation >= 0) {
					mr.material = MatGrasslands;
				} else {
					mr.material = MatOcean;
				}

				MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter> ();
				mf.mesh = MeshWater;
			}
		}
	}
}
