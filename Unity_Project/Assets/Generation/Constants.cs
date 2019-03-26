public static class Constants {

    //Floor stuff
    public const int FLOOR_WIDTH = 76; //these two in number of tiles
    public const int FLOOR_HEIGHT = 76; //16:9 ish
    public const int TILE_WIDTH = 32; //these two in pixels
    public const int TILE_HEIGHT = 32;

	//Room Generation
	public const int ROOM_ATTEMPTS = 60;
	public const int ROOM_MIN_WIDTH = 5;
    public const int ROOM_MIN_HEIGHT = 5;
	public const int MAX_ROOM_SIZE = 25;
	public const int ROOM_SIZE_MEAN = MAX_ROOM_SIZE / 2;
	public const float ROOM_SIZE_DEVIATION = (float) 2.7;
	public const int ROOM_GAP = 3;

	//Pathing
	public const int PATHS_MIN = 10;
	public const int PATHS_MAX = 10;
	public const int PATH_DIR_CHANGE_INTERVAL_MIN = 1;
	public const int PATH_DIR_CHANGE_INTERVAL_MAX = 10;

}
