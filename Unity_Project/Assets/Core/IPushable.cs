using UnityEngine;

namespace TimefulDungeon.Core {
    public interface IPushable {
        public void Push(Vector2 awayFromPoint, float magnitude);
    }
}