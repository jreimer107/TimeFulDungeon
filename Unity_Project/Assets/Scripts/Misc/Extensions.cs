public static class Extensions {
	public static int Clamp(this int value, int inclusiveMin,
		int exclusiveMax) {
		if (inclusiveMin == exclusiveMax) {
			return inclusiveMin;
		} else if (value < inclusiveMin) {
			return inclusiveMin;
		} else if (value >= exclusiveMax) {
			return exclusiveMax - 1;
		} else {
			return value;
		}
	}

	public static int Min(this int x, int y) {
		return (x > y) ? y : x;
	}

	public static int Max(this int x, int y) {
		return (x > y) ? x : y;
	}
}