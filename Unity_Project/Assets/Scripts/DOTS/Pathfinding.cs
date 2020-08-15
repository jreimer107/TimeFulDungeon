using UnityEngine;
using Unity.Collections;
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
			// jobHandles.Add(findPathJob.Schedule());
			Debug.ClearDeveloperConsole();
			findPathJob.Run();
			PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);
		});
		// JobHandle.CompleteAll(jobHandles);
		jobHandles.Dispose();


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
			// A Star pathfinding. Finds rough path between two points.
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

				NativeList<int2> succesors = GetSuccessors(curr);
				for (int i = 0; i < succesors.Length; i++) {
					int2 suc = succesors[i];
					if (!IsWalkable(suc.x, suc.y) || closed.ContainsKey(suc)) {
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

			// If we didn't find a path, dispose and return
			if (!curr.Equals(end)) {
				open.Dispose();
				closed.Dispose();
				parents.Dispose();
				costs.Dispose();
				return;
			}

			// If we did find a path, extract path from parents hashmap. Then dispose and continue.
			while (!curr.Equals(nothing)) {
				path.Add(curr);
				curr = parents[curr];
			}
			open.Dispose();
			closed.Dispose();
			parents.Dispose();
			costs.Dispose();
			// End of A Star

			// Waypointifying. Removes unnecessary positions from A Star result
			/*
			curr = start 
			while curr is not end
				turn = last node in path where turn.x == curr.x || turn.y == curr.y
				while all nodes between curr and turn are walkable
					turn = next node in path
				waypoints.add(node before turn)
				curr = node before turn
			*/
			NativeList<int2> waypoints = new NativeList<int2>(Allocator.Temp);
			curr = start;
			int currIndex = path.Length - 1;
			// Debug.LogFormat("Waypointifying path from {0} to {1}", start, end);
			// Until we're testing the end waypoint
			while (!curr.Equals(end)) {
				// The next waypoint is the last node after a turn that is still visible from the current waypoint
				// Debug.Log($"Finding next waypoint from {curr}, {currIndex}");

				// Find the first node after a turn
				int turnIndex = currIndex;
				int2 turn = path[turnIndex];
				while (curr.x == turn.x || curr.y == turn.y || math.abs(turn.x - curr.x) == math.abs(turn.y - curr.y)) {
					// Debug.Log($"Turnskip {turn}, {turnIndex}");
					if (turnIndex == 0) {
						// Debug.Log("Turn finder reached end of path.");
						curr = end;
						break;
					}
					turn = path[--turnIndex];
				}

				// If there's a turn on the last position, the inSight check won't happen. Add the second-last position.
				if (turnIndex == 0) {
					waypoints.Add(path[turnIndex + 1]);
				}
				// Debug.Log($"Turn {turn} found at index {turnIndex}");

				// Find the last node after the turn that is still visible from the current waypoint
				bool inSight = true;
				while (inSight && turnIndex > 0) {
					// Check if the node after the turn is visible from the current waypoint
					// Check if all nodes between the post-turn node and the current waypoint are walkable
					// Debug.Log($"Possible waypoint is {turn}, {delta} away, slope of {slope}");
					float2 delta = turn - curr;
					float slope = delta.y / delta.x;
					if (math.abs(slope) >= 1) {
						// If slope > 1, path crosses at least one y border for each x.
						// So for each y crossed, check that the positions left or right walkable, based on where the entity is
						Debug.LogFormat("Slope > 1: {0}", slope);
						int x, y = 0;
						float xf;
						int2 test;
						while (y != delta.y) {
							// Get the exact and rounded x coordinates
							xf = (float)y / slope;
							x = (int)math.trunc(xf);
							test.x = curr.x + x;
							test.y = curr.y + y;

							// If exactX < roundedX, entity is on right half of square. Check square left of current pos.
							// If exactX > roundedX, entity is on left half of square. Check square right of current pos.
							// Debug.LogFormat("Testing walkability of ({0}, {1})", x, y);
							if (!IsWalkable(test) ||
								xf < x && !IsWalkable(test.x - 1, test.y) ||
								xf > x && !IsWalkable(test.x + 1, test.y)) {
								inSight = false;
								break;
							}
							
							// Move up or down based on slope
							y += delta.y > 0 ? 1 : -1;
						}
					} else {
						// Debug.Log("Slope < 1:" + slope);
						// If slope < 1, path crosses at least one x for each y.
						// For each x crossed, check that the position up or down is walkable, based on where the entity is
						int x = 0, y;
						float yf;
						int2 test;
						while (x != delta.x) {
							// Get the exact and rounded y coordinates
							yf = x * slope;
							y = (int)math.trunc(yf);
							test.x = curr.x + x;
							test.y = curr.y + y;

							// Debug.LogFormat("Testing walkability of ({0}, {1})", test.x, test.y);
							// If exactY < roundedY, entity is on bottom half of square. Check the square below.
							// If exactY > roundedY, entity is on top half of square. Check the square above.
							if (!IsWalkable(test) ||
								yf < y && !IsWalkable(test.x, test.y - 1) ||
								yf > y && !IsWalkable(test.x, test.y + 1)) {
								inSight = false;
								break;
							}
							
							// Move left or right based on slope
							x += delta.x > 0 ? 1 : -1;
						}
					}

					// If no collision was found, then the path between this post-turn node and the current waypoint is walkable.
					// Try again with the next post-turn node.
					if (inSight) {
						turn = path[--turnIndex];
					} else {
						// Only a bad turn fails this condition, so move one back. The square before a bad turn is a waypoint.
						turn = path[++turnIndex];
					}
				}

				// Start again at the next waypoint, add it to the waypoint list.
				curr = turn;
				currIndex = turnIndex;
				waypoints.Add(curr);
				// Debug.LogFormat("Adding waypoint {0}", curr);
			}

			// Add waypoints to path buffer so that external processes can access.
			path.Clear();
			for (int i = waypoints.Length - 1; i >= 0; i--) {
				path.Add(waypoints[i]);
				// Debug.LogFormat("Waypoint: {0}", waypoints[i]);
			}
			waypoints.Dispose();
			// End of waypointifying
		}

		public bool IsWalkable(int2 p) {
			if (!Utils.PointInBox(p, gridSize)) {
				return false;
			}
			return walkableGrid[p.x * gridSize.x + p.y];
		}
		public bool IsWalkable(int x, int y) => IsWalkable(new int2(x, y));

		public NativeList<int2> GetSuccessors(int2 p) {
			NativeList<int2> ret = new NativeList<int2>(4, Allocator.Temp);
			ret.Add(new int2(p.x + 1, p.y));
			ret.Add(new int2(p.x, p.y + 1));
			ret.Add(new int2(p.x - 1, p.y));
			ret.Add(new int2(p.x, p.y - 1));

			if (IsWalkable(p.x + 1, p.y) && IsWalkable(p.x, p.y + 1)) {
				ret.Add(new int2(p.x + 1, p.y + 1));
			}
			if (IsWalkable(p.x, p.y + 1) && IsWalkable(p.x - 1, p.y)) {
				ret.Add(new int2(p.x - 1, p.y + 1));
			}
			if (IsWalkable(p.x - 1, p.y) && IsWalkable(p.x, p.y - 1)) {
				ret.Add(new int2(p.x - 1, p.y - 1));
			}
			if (IsWalkable(p.x, p.y - 1) && IsWalkable(p.x + 1, p.y)) {
				ret.Add(new int2(p.x + 1, p.y - 1));
			}
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