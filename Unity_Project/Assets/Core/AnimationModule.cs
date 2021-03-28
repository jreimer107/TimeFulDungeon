using TimefulDungeon.Core.Movement;
using UnityEngine;

namespace TimefulDungeon.Core {
    [RequireComponent(typeof(SpriteRenderer), typeof(Animator), typeof(MovementController))]
    public class AnimationModule : MonoBehaviour {
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Vertical = Animator.StringToHash("Vertical");
        private bool _hasHorizontalAnimation;
        private bool _hasVerticalAnimation;
        private Animator _animator;
        private MovementController _movementController;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;

        private bool FacingRight {
            get => _spriteRenderer.flipX;
            set => _spriteRenderer.flipX = value;
        }

        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _movementController = GetComponent<MovementController>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            foreach (var parameter in _animator.parameters) {
                switch (parameter.name) {
                    case "Horizontal":
                        _hasHorizontalAnimation = true;
                        break;
                    case "Vertical":
                        _hasVerticalAnimation = true;
                        break;
                }

                if (_hasHorizontalAnimation && _hasVerticalAnimation) break;
            }
        }

        private void Update() {
            var desiredVelocity = _movementController.DesiredDirection * _rigidbody.velocity.magnitude;
            if (_hasHorizontalAnimation)
                _animator.SetFloat(Horizontal, Mathf.Abs(desiredVelocity.x));
            if (_hasVerticalAnimation)
                _animator.SetFloat(Vertical, Mathf.Abs(desiredVelocity.y));

            // Flip animation based on intended direction
            if (desiredVelocity.x != 0 && (desiredVelocity.x < 0) ^ FacingRight) FacingRight = !FacingRight;
        }
    }
}