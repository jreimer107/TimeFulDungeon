using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class AnimationModule : MonoBehaviour {
	#region Private fields
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private bool hasVerticalAnimation;
	private bool hasHorizontalAnimation;
	private bool facingRight { get => spriteRenderer.flipX; set => spriteRenderer.flipX = value; }
	#endregion

	#region Public fields
	/// <summary>
	/// Input. Determines what animation plays. Assign at creation.
	/// </summary>
	[HideInInspector] public Func<Vector2> GetDesiredVelocity = () => {
		Debug.LogWarning("No velocity getter given to AnimationModule!");
		return Vector2.zero;
	};
	#endregion

	#region Unity methods
	private void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		foreach (AnimatorControllerParameter parameter in animator.parameters) {
			if (parameter.name == "Horizontal") {
				hasHorizontalAnimation = true;
			} else if (parameter.name == "Vertical") {
				hasVerticalAnimation = true;
			}
			if (hasHorizontalAnimation && hasVerticalAnimation) {
				break;
			}
		}
	}

	private void Update() {
		// Update 
		Vector2 desiredVelocity = GetDesiredVelocity();
		if (hasHorizontalAnimation)
			animator.SetFloat("Horizontal", Mathf.Abs(desiredVelocity.x));
		if (hasVerticalAnimation)
			animator.SetFloat("Vertical", Mathf.Abs(desiredVelocity.y));

		// Flip animation based on intended direction
		if (desiredVelocity.x != 0 && (desiredVelocity.x < 0 ^ facingRight)) {
			facingRight = !facingRight;
		}
	}
	#endregion
}
