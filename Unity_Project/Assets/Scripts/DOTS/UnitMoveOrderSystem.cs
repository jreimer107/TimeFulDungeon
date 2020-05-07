using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class UnitMoveOrderSystem : ComponentSystem {

	protected override void OnUpdate() {
		if (Input.GetMouseButtonDown(0)) {
			Entities.ForEach((Entity entity, ref Translation translation) => {
				EntityManager.AddComponentData(entity, new PathfindingParams {
					start = new int2(0, 0),
					end = new int2(50, 40)
				});
			});
		}		
	}
}
