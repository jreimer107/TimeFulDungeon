using TimefulDungeon.Core;
using TimefulDungeon.Misc;
using UnityEngine;
using VoraUtils;

public class Tester : MonoBehaviour {
	// Start is called before the first frame update
	private WorldGrid<bool> grid;

	void Start() {
		grid = new WorldGrid<bool>(75, 75, 1f);
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 mousePos = Utils.GetMouseWorldPosition2D();
			grid.Set(mousePos, !grid.Get(mousePos));
		}
	}
}
