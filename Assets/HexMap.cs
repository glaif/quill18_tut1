using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		generateMap ();
	}

	public GameObject hexPrefab;

    public Material[] hexMaterials;

    public readonly int NumRows = 60;
    public readonly int NumCols = 120;

	public void generateMap() {
		for (int col = 0; col < NumCols; col++) {
			for (int row = 0; row < NumRows; row++) {

                Hex h = new Hex(col, row);

                Vector3 pos = h.PositionFromCamera(
                    Camera.main.transform.position,
                    NumRows,
                    NumCols
                );

				GameObject hexGO = Instantiate(
                    hexPrefab, 
                    pos, 
                    Quaternion.identity, 
                    this.transform);
                hexGO.GetComponent<HexComponent>().Hex = h;
                hexGO.GetComponent<HexComponent>().HexMap = this;

                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                mr.material = hexMaterials[Random.Range(0, hexMaterials.Length)];
			}
		}

        //StaticBatchingUtility.Combine(this.gameObject);
	}
}
