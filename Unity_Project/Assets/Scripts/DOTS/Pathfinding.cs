using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using System;
using Unity.Entities;
using System.Collections.Generic;

public class Pathfinding : ComponentSystem {

	protected override void OnUpdate() {
		NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);
		List<FindPathJob> findPathJobs = new List<FindPathJob>();
		float startTime = UnityEngine.Time.realtimeSinceStartup;
		Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> path, ref PathfindingParams pathfindingParams) => {
			// Debug.Log("Find path!");
			FindPathJob findPathJob = new FindPathJob
			{
				start = pathfindingParams.start,
				end = pathfindingParams.end,
				path = new NativeList<int2>(Allocator.TempJob),
				gridSize = new int2(PathfindingGrid.Instance.width, PathfindingGrid.Instance.height),
				walkableGrid = PathfindingGrid.Instance.GetNativeArray(Allocator.TempJob),
				entity = entity
			};
			findPathJobs.Add(findPathJob);
			jobHandles.Add(findPathJob.Schedule());
			PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);
		});
		JobHandle.CompleteAll(jobHandles);


		foreach (FindPathJob findPathJob in findPathJobs) {
			new FillBufferJob
			{
				path = findPathJob.path,
				entity = findPathJob.entity,
				pathfollowFromEntity = GetComponentDataFromEntity<PathFollow>(),
				pathPositionBufferFromEntity = GetBufferFromEntity<PathPosition>()
			}.Run();
			findPathJob.path.Dispose();
		}
	}

	[BurstCompile]
	private struct FindPathJob : IJob {
		public int2 start;
		public int2 end;
		public int2 gridSize;

		[DeallocateOnJobCompletion]
		public NativeArray<bool> walkableGrid;

		// TODO: Once this buffer issue is addressed, combine these two jobs.
		public NativeList<int2> path; // Input for sequential buffer job
		public Entity entity; // Data storage for sequential buffer job

		public void Execute() {
			GetPath();
		}

		private void GetPath() {
			NativeMinHeap<MinHeapNode> open = new NativeMinHeap<MinHeapNode>(Allocator.Temp);
			NativeHashMap<int2, Utils.Empty> closed = new NativeHashMap<int2, Utils.Empty>(16, Allocator.Temp);

			NativeHashMap<int2, int2> parents = new NativeHashMap<int2, int2>(16, Allocator.Temp);
			NativeHashMap<int2, float> costs = new NativeHashMap<int2, float>(16, Allocator.Temp);

			int2 nothing = new int2(-1, -1);

			open.Add(new MinHeapNode(start, GetHeuristic(start, end)));
			int2 curr = nothing;
			parents[start] = nothing;
			costs[start] = 0;
			while (!open.IsEmpty()) {
				curr = open.Pop().position;

				if (curr.Equals(end)) {
					break;
				}
				closed.TryAdd(curr, new Utils.Empty());

				NativeArray<int2> succesors = GetSuccessors(curr);
				for (int i = 0; i < succesors.Length; i++) {
					int2 suc = succesors[i];
					if (!Utils.PointInBox(suc, gridSize) || closed.ContainsKey(suc) || !walkableGrid[suc.x * gridSize.x + suc.y]) {
						continue;
					}

					float newSucCost = GetCost(curr, suc, costs[curr]);
					float currentSucCost = 0;
					if (!costs.TryGetValue(suc, out currentSucCost) || newSucCost < currentSucCost) {
						open.Add(new MinHeapNode(suc, newSucCost + GetHeuristic(suc, end)));
						costs[suc] = newSucCost;
						parents[suc] = curr;
					}
				}
				succesors.Dispose();
			}

			while (curr.x != nothing.x || curr.y != nothing.y) {
				path.Add(curr);
				curr = parents[curr];
			}

			open.Dispose();
			closed.Dispose();
			parents.Dispose();
			costs.Dispose();
		}

		public NativeArray<int2> GetSuccessors(int2 p) {
			NativeArray<int2> ret = new NativeArray<int2>(4, Allocator.Temp);
			ret[0] = new int2(p.x + 1, p.y);
			ret[1] = new int2(p.x, p.y + 1);
			ret[2] = new int2(p.x - 1, p.y);
			ret[3] = new int2(p.x, p.y - 1);
			// ret[0] = new int2(p.x + 1, p.y);
			// ret[1] = new int2(p.x + 1, p.y + 1);
			// ret[2] = new int2(p.x, p.y + 1);
			// ret[3] = new int2(p.x - 1, p.y + 1);
			// ret[4] = new int2(p.x - 1, p.y);
			// ret[5] = new int2(p.x - 1, p.y - 1);
			// ret[6] = new int2(p.x, p.y - 1);
			// ret[7] = new int2(p.x + 1, p.y - 1);
			return ret;
		}

		public float GetCost(int2 c, int2 s, float cCost) => cCost + Utils.Distance(c, s);

		public float GetHeuristic(int2 c, int2 e) => Utils.Distance(c, e);
	}

	private struct MinHeapNode : IComparable<MinHeapNode> {
		public MinHeapNode(int2 position, float heuristic) {
			this.position = position;
			this.heuristic = heuristic;
		}

		public int2 position { get; }
		public float heuristic { get; }
		public int x => position.x;
		public int y => position.y;

		public int CompareTo(MinHeapNode other) => this.heuristic.CompareTo(other.heuristic);

		public override string ToString() => string.Format("({0}, {1})", position, heuristic);
	}
}

/// <summary>
/// THIS JOB SHOULD BE REMOVED IN THE FUTURE.
/// Dynamic buffers cannot be modified in a parallel environment. This job takes a NativeList,
/// which can, and populates the Dynamic buffer in a sequential state.
/// </summary>
[BurstCompile]
public struct FillBufferJob : IJob {
	// [DeallocateOnJobCompletion]
	public NativeList<int2> path;

	public Entity entity;
	public ComponentDataFromEntity<PathFollow> pathfollowFromEntity;
	public BufferFromEntity<PathPosition> pathPositionBufferFromEntity;

	public void Execute() {
		DynamicBuffer<PathPosition> pathPositions = pathPositionBufferFromEntity[entity];
		pathPositions.Clear();

		for (int i = 0; i < path.Length; i++) {
			pathPositions.Add(new PathPosition { position = path[i] });
		}
		pathfollowFromEntity[entity] = new PathFollow { pathIndex = path.Length - 1 };
	}
}