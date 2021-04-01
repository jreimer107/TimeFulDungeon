using System;

namespace TimefulDungeon.Core.FiniteStateMachine {
    public interface IState<out T> where T : Enum {
        public T Name { get; }
        public bool CanEnter();
        public void Start();
        public T Update();
        public void Exit();
    }
}