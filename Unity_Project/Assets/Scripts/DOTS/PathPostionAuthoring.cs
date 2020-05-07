using UnityEngine;
using Unity.Entities;

public class PathPostionAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
		dstManager.AddBuffer<PathPosition>(entity);
	}
}