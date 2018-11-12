using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Room {
    public int x_pos;
    public int y_pos;
    public int width;
    public int height;
	public List<Path> connectedPaths;

	//Bounds are tiles taken up by the room
	public int UpperBound {
		get { return y_pos + height; }
	}
	public int LowerBound {
		get { return y_pos; }
	}
	public int LeftBound {
		get { return x_pos; }
	}
	public int RightBound {
		get { return x_pos + width; }
	}

	//Spaces are the areas where another room cannot be (too close to this one)
	public int UpperSpace {
		get { return UpperBound + Constants.ROOM_GAP; }
	}
	public int LowerSpace {
		get { return LowerBound - Constants.ROOM_GAP; }
	}
	public int LeftSpace {
		get { return LeftBound - Constants.ROOM_GAP; }
	}
	public int RightSpace {
		get { return RightBound + Constants.ROOM_GAP; }
	}

	public Room (int x, int y, int w, int h) {
        x_pos = x;
        y_pos = y;
        width = w;
        height = h;
		connectedPaths = new List<Path>();
	}

	public Coordinate GetRandCoordinate() {
		int randX = GetRandXPos();
		int randY = GetRandYPos();
		return new Coordinate(randX, randY);
	}

	public int GetRandXPos() {
		Random rng = new Random();
		return rng.Next(LeftBound, RightBound);
	}
	public int GetRandYPos() {
		Random rng = new Random();
		return rng.Next(LowerBound, UpperBound);
	}

	//Coalesce the connectedRooms lists of each path connected to this room
	public void CoalesceConnections(Room other) {
		foreach (Path p in connectedPaths) {
			other.connectedPaths.Add(p);
		}
		connectedPaths = other.connectedPaths;
	}
}
