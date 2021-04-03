using UnityEngine;

namespace TimefulDungeon.Misc {
	/// <summary>
	///     Class for shaping dot products.
	///     Creates different behaviors for various angles.
	///     Each function inputs two normalized Vector2s and maybe some other values.
	///     Each function outputs a normalized float based on how much the shaping function likes the angle.
	/// </summary>
    public class ShapingFunctions {
        	    /// <summary>
	    ///     Likes small angles, or similar vectors.
	    /// </summary>
	    /// <param name="to">1st vector</param>
	    /// <param name="from">2nd vector</param>
	    /// <returns>Shaping coefficient. 1 for same angle, -1 for opposite, 0 for perpendicular.</returns>
	    public static float Positive(Vector2 to, Vector2 from) {
            return Vector2.Dot(to, from);
        }

	    /// <summary>
	    ///     Likes large angles, opposite vectors.
	    /// </summary>
	    /// <param name="to">1st vector</param>
	    /// <param name="from">2nd vector</param>
	    /// <returns>-1 for same angle, 1 for opposite, 0 for perpendicular.</returns>
	    public static float Negative(Vector2 to, Vector2 from) {
            return -Positive(to, from);
        }

	    /// <summary>
	    ///     Likes angles similar to given angle.
	    /// </summary>
	    /// <param name="to">1st vector</param>
	    /// <param name="from">2nd vector</param>
	    /// <param name="radians">The preferred angle in radians.</param>
	    /// <returns>1 for same as given angle, -1 for opposite, 0 for perpendicular.</returns>
	    public static float Rotate(Vector2 to, Vector2 from, float radians) {
            return Positive(to.Rotate(radians), from);
        }

	    /// <summary>
	    ///     Lessens the weights of values the further positive or negative the offset is. For example, an offset
	    ///     of 1 sets negative results to 0.
	    /// </summary>
	    /// <param name="to">1st vector</param>
	    /// <param name="from">2nd vector</param>
	    /// <param name="offset">Offset to apply. Valid results only between 1 and -1.</param>
	    /// <returns>The result of the dot product, squished in a certain direction based on the offset.</returns>
	    public static float PositiveOffset(Vector2 to, Vector2 from, float offset = 1f) {
            return (Positive(to, from) + offset) / (1 + offset);
        }

	    /// <summary>
	    ///     Lessens the weights of values the further positive or negative the offset is. For example, an offset
	    ///     of 1 sets positive results to 0.
	    /// </summary>
	    /// <param name="to">1st vector</param>
	    /// <param name="from">2nd vector</param>
	    /// <param name="offset">Offset to apply. Valid results only between 1 and -1.</param>
	    /// <returns>The result of the dot product, squished in a certain direction based on the offset.</returns>
	    public static float NegativeOffset(Vector2 to, Vector2 from, float offset = 1f) {
            return (Negative(to, from) + offset) / (1 + offset);
        }

	    /// <summary>
	    ///     Likes perpendicular angles.
	    /// </summary>
	    /// <param name="to">1st vector</param>
	    /// <param name="from">2nd vector</param>
	    /// <returns>0 for same and opposite angles, 1 for perpendicular.</returns>
	    public static float Perpendicular(Vector2 to, Vector2 from) {
            return Mathf.Abs(Rotate(to, from, Mathf.PI / 2));
        }

        public static float FavorSides(Vector2 to, Vector2 from, float favor = 0.5f) {
            // return 1 - Mathf.Abs(Vector2.Dot(to, from) - favor);
            return NegativeOffset(to, from, 1 - favor);
        }

        /// <summary>
        ///     Special combination such that same-ish and perpendicular angles are strongly positive,
        ///     but opposite directions are still negative.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static float AnywhereButBack(Vector2 to, Vector2 from) {
            return Positive(to, from) + 0.5f * Perpendicular(to, from);
        }

        public static float AnywhereButThere(Vector2 to, Vector2 from) {
            return Negative(to, from) + 0.5f * Perpendicular(to, from);
        }
    }
}