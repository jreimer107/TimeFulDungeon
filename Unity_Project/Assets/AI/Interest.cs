using UnityEngine;

namespace TimefulDungeon.AI {
    public struct Interest {
        private readonly Transform _transform;
        private Vector2 _vector2;

        public Vector2 Position {
            get => _transform ? _transform.Position2D() : _vector2;
            set => _vector2 = value;
        }

        public readonly bool isDanger;

        public Interest(Transform transform, bool isDanger = false) {
            _transform = transform;
            _vector2 = Vector2.zero;
            this.isDanger = isDanger;
        }

        public Interest(Vector2 vector2, bool isDanger = false) {
            _transform = null;
            _vector2 = vector2;
            this.isDanger = isDanger;
        }

        public static implicit operator Vector2(Interest i) {
            return i.Position;
        }
    }
}