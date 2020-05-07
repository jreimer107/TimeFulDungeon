using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class PathFollowSystem : ComponentSystem {
	protected override void OnUpdate() {
		Entities.ForEach((DynamicBuffer<PathPosition> path, ref Translation translation, ref PathFollow pathFollow) => {
			int index = pathFollow.pathIndex;
			if (index >= 0) {
				int2 pathPosition = path[index].position;

				float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
				float3 moveDir = math.normalizesafe(targetPosition - translation.Value);
				float moveSpeed = 3f;

				translation.Value += moveDir * moveSpeed * Time.DeltaTime;
				
				if (math.distance(translation.Value, targetPosition) < 0.1f) {
					pathFollow.pathIndex--;
				}
			}
		});
	}
}
