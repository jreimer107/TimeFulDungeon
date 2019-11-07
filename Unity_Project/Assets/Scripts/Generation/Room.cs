using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;

public class Room {
	//publics to be accessed by other classes
	public int x_pos;
	public int y_pos;
	public int width;
	public int height;
	public List<Path> connectedPaths;
	private Random rng;

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
		get { return UpperBound + BoardHolder.instance.genConfig.RoomGap; }
	}
	public int LowerSpace {
		get { return LowerBound - BoardHolder.instance.genConfig.RoomGap; }
	}
	public int LeftSpace {
		get { return LeftBound - BoardHolder.instance.genConfig.RoomGap; }
	}
	public int RightSpace {
		get { return RightBound + BoardHolder.instance.genConfig.RoomGap; }
	}

	public Room(int x, int y, int w, int h) {
		x_pos = x;
		y_pos = y;
		width = w;
		height = h;
		connectedPaths = new List<Path>();
		rng = new Random();
	}


	public Coordinate GetRandEntrance() {
		int side = this.rng.Next(3);
		if (side == 0) { //Left side
			return new Coordinate(this.x_pos - 1, this.GetRandYPos());
		} else if (side == 1) { //Top
			return new Coordinate(this.GetRandXPos(), this.y_pos + this.height);
		} else if (side == 2) { //Right
			return new Coordinate(this.x_pos + this.width, this.GetRandYPos());
		} else { //Bottom
			return new Coordinate(this.GetRandXPos(), this.y_pos - 1);
		}
	}

	public Coordinate GetRandCoordinate() {
		int randX = GetRandXPos();
		int randY = GetRandYPos();
		return new Coordinate(randX, randY);
	}

	public int GetRandXPos() {
		return rng.Next(LeftBound, RightBound);
	}
	public int GetRandYPos() {
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
