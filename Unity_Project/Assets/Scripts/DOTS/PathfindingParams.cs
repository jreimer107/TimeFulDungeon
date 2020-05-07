using Unity.Entities;
using Unity.Collections;
using System;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PathfindingParams : IComponentData {
	public int2 start;
	public int2 end;
}
