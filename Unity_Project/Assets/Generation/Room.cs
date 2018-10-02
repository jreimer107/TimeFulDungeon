using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
    public int x_pos;
    public int y_pos;
    public int width;
    public int height;

	public Room (int x, int y, int w, int h) {
        this.x_pos = x;
        this.y_pos = y;
        this.width = w;
        this.height = h;
	}
}
