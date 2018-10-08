using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    public int x_pos;
    public int y_pos;
    public int width;
    public int height;
	public bool connected;

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
		get { return y_pos + width; }
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
        this.x_pos = x;
        this.y_pos = y;
        this.width = w;
        this.height = h;
		this.connected = false;
	}
}
