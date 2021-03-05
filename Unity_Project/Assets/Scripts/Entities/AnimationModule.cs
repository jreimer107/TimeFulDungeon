using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class AnimationModule : MonoBehaviour {
	#region Private fields
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	private bool hasVerticalAnimation;
	private bool hasHorizontalAnimation;
	private bool FacingRight { get => spriteRenderer.flipX; set => spriteRenderer.flipX = value; }
	#endregion

	#region Public fields
	/// <summary>
	/// Input. Determines what animation plays. Assign at creation.
	/// </summary>
	public Func<Vector2> getDesiredVelocity = () => {
		Debug.LogWarning("No velocity getter given to AnimationModule!");
		return Vector2.zero;
	};

	private static readonly int Horizontal = Animator.StringToHash("Horizontal");
	private static readonly int Vertical = Animator.StringToHash("Vertical");

	#endregion

	#region Unity methods
	private void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		foreach (var parameter in animator.parameters)  {
			switch (parameter.name)  {
				case "Horizontal":
					hasHorizontalAnimation = true;
					break;
				case "Vertical":
					hasVerticalAnimation = true;
					break;
			}

			if (hasHorizontalAnimation && hasVerticalAnimation) {
				break;
			}
		}
	}

	private void Update() {
		// Update 
		var desiredVelocity = getDesiredVelocity();
		if (hasHorizontalAnimation)
			animator.SetFloat(Horizontal, Mathf.Abs(desiredVelocity.x));
		if (hasVerticalAnimation)
			animator.SetFloat(Vertical, Mathf.Abs(desiredVelocity.y));

		// Flip animation based on intended direction
		if (desiredVelocity.x != 0 && (desiredVelocity.x < 0 ^ FacingRight)) {
			FacingRight = !FacingRight;
		}
	}
	#endregion
}
