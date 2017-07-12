using QPath;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour, IQPathWorld {

	// Use this for initialization
	void Start ()
	{
		generateMap ();
	}

    void Update() {
        //TESTING: Hit spacebar to advance to next turn
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (units != null) {
                foreach(Unit u in units) {
                    u.DoTurn();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            if (units != null) {
                foreach (Unit u in units) {
                    u.DUMMY_PATHING_FUNCTION();
                }
            }
        }
    }

    public GameObject hexPrefab;

	public Mesh MeshWater;
	public Mesh MeshFlat;
	public Mesh MeshHill;
	public Mesh MeshMountain;

    public GameObject ForestPrefab;
    public GameObject JunglePrefab;

    public Material MatOcean;
	public Material MatPlains;
	public Material MatGrasslands;
	public Material MatMountains;
    public Material MatDesert;

    public GameObject UnitDwarfPrefab;

    // thresholds to determine what a tile is based on height
    [System.NonSerialized] public float HeightMountain = 1f;
    [System.NonSerialized] public float HeightHill = 0.6f;
    [System.NonSerialized] public float HeightFlat = 0.0f;

    [System.NonSerialized] public float MoistureJungle = 1f;
    [System.NonSerialized] public float MoistureForest = 0.5f;
    [System.NonSerialized] public float MoistureGrasslands = 0.0f;
    [System.NonSerialized] public float MoisturePlains = -0.75f;

    [System.NonSerialized] public readonly int NumRows = 30;
    [System.NonSerialized] public readonly int NumCols = 60;

    // TODO: Link with Hex version of this
    [System.NonSerialized] public bool allowWrapEastWest = true;
    [System.NonSerialized] public bool allowWrapNorthSouth = false;

	private Hex[,] hexes;
	private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;

    private HashSet<Unit> units;
    private Dictionary<Unit, GameObject> unitToGameObjectMap;

    public Hex GetHexAt (int x, int y)
	{
		if (hexes == null) {
			Debug.LogError ("Hexes array not yet instantiated!");
			return null;	
		}

		if (allowWrapEastWest) {
            x = x % NumCols;
            if (x < 0)
                x += NumCols;
        }


        if (allowWrapNorthSouth) {
            y = y % NumRows;
            if (y < 0)
                y += NumRows;
        }
		
		return hexes [x, y];
	}

    public Hex GetHexFromGameObject(GameObject hexGO) {
        if (gameObjectToHexMap.ContainsKey(hexGO)) {
            return gameObjectToHexMap[hexGO];
        }
        return null;
    }

    public GameObject GetHexGO(Hex h) {
        if (hexToGameObjectMap.ContainsKey(h)) {
            return hexToGameObjectMap[h];
        }
        return null;
    }

    public Vector3 GetHexPosition(int q, int r) {
        Hex hex = GetHexAt(q, r);
        return GetHexPosition(hex);
    }

    public Vector3 GetHexPosition(Hex hex) {
        return hex.PositionFromCamera(Camera.main.transform.position, NumRows, NumCols);
    }

    virtual public void generateMap ()
	{
		// Generate a map filled with ocean

		hexes = new Hex[NumCols, NumRows];
		hexToGameObjectMap = new Dictionary<Hex, GameObject> ();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

        for (int col = 0; col < NumCols; col++) {
			for (int row = 0; row < NumRows; row++) {

				Hex h = new Hex (this, col, row);

				hexes [col, row] = h;
				h.Elevation = -0.5f;

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
				
				hexToGameObjectMap[h] = hexGO;
                gameObjectToHexMap[hexGO] = h;

                h.TerrainType = Hex.TERRAIN_TYPE.OCEAN;
                h.ElevationType = Hex.ELEVATION_TYPE.WATER;

				hexGO.name = string.Format ("HEX: {0},{1}", col, row);
				hexGO.GetComponent<HexComponent> ().Hex = h;
				hexGO.GetComponent<HexComponent> ().HexMap = this;
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
				GameObject hexGO = hexToGameObjectMap[h];

				MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();

                if (h.Elevation >= HeightFlat && h.Elevation < HeightMountain) {
                    if (h.Moisture >= MoistureJungle) {
                        mr.material = MatGrasslands;
                        h.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
                        h.FeatureType = Hex.FEATURE_TYPE.RAINFOREST;

                        // Spawn Trees
                        Vector3 p = hexGO.transform.position;
                        if (h.Elevation >= HeightHill) {
                            p.y += 0.25f;
                        }
                        GameObject.Instantiate(JunglePrefab, p, Quaternion.identity, hexGO.transform);

                    } else if (h.Moisture >= MoistureForest) {
                        mr.material = MatGrasslands;
                        h.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
                        h.FeatureType = Hex.FEATURE_TYPE.FOREST;

                        // Spawn Forest
                        Vector3 p = hexGO.transform.position;
                        if (h.Elevation >= HeightHill) {
                            p.y += 0.25f;
                        }
                        GameObject.Instantiate(ForestPrefab, p, Quaternion.identity, hexGO.transform);

                    } else if (h.Moisture >= MoistureGrasslands) {
                        mr.material = MatGrasslands;
                        h.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;

                    } else if (h.Moisture >= MoisturePlains) {
                        mr.material = MatPlains;
                        h.TerrainType = Hex.TERRAIN_TYPE.PLAINS;

                    } else {
                        mr.material = MatDesert;
                        h.TerrainType = Hex.TERRAIN_TYPE.DESERT;
                    }
                }

                if (h.Elevation >= HeightMountain) {
                    mr.material = MatMountains;
                    mf.mesh = MeshMountain;
                    h.ElevationType = Hex.ELEVATION_TYPE.MOUNTAIN;

                } else if (h.Elevation >= HeightHill) {
                    mf.mesh = MeshHill;
                    h.ElevationType = Hex.ELEVATION_TYPE.HILL;

                } else if (h.Elevation >= HeightFlat) {
                    mf.mesh = MeshFlat;
                    h.ElevationType = Hex.ELEVATION_TYPE.FLAT;

                } else {
                    mr.material = MatOcean;
                    mf.mesh = MeshWater;
                    h.ElevationType = Hex.ELEVATION_TYPE.WATER;
                }

                hexGO.GetComponentInChildren<TextMesh>().text = 
                    string.Format("{0},{1}\n{2}", col, row, h.BaseMovementCost(false, false, false));
            }
		}
	}

    public Hex[] GetHexesWithinRangeOf(Hex centerHex, int range) {
        List<Hex> results = new List<Hex>();

        for (int dx = -range; dx < range-1; dx++) {
            for (int dy = Mathf.Max(-range+1, -dx-range); dy <= Mathf.Min(range, -dx+range-1); dy++) {
                results.Add(GetHexAt(centerHex.Q + dx, centerHex.R + dy));
            }
        }
        return results.ToArray();
    }

    public void SpawnUnitAt(Unit unit, GameObject prefab, int q, int r) {

        if (units == null) {
            units = new HashSet<Unit>();
            unitToGameObjectMap = new Dictionary<Unit, GameObject>();
        }
        Hex myHex = GetHexAt(q, r);
        GameObject myHexGO = hexToGameObjectMap[myHex];
        unit.SetHex(myHex);
        GameObject unitGO = Instantiate(prefab, myHexGO.transform.position, Quaternion.identity, myHexGO.transform);
        unit.OnUnitMoved += unitGO.GetComponent<UnitView>().OnUnitMoved;

        units.Add(unit);
        unitToGameObjectMap.Add(unit, unitGO);
    }
}
