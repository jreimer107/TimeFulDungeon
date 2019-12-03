public static class Extensions {
	public static int Clamp(this int value, int inclusiveMin,
		int exclusiveMax) {
		if (value < inclusiveMin) {
			return inclusiveMin;
		} else if (value >= exclusiveMax) {
			return exclusiveMax - 1;
		} else {
			return value;
		}
	}
}