using System;

namespace TimefulDungeon.Core.FiniteStateMachine {
    public class SubStateMachine<T, E> : FiniteStateMachine<T>, IState<E> where T : Enum where E : Enum {
        public E Name { get; protected set; }

        public SubStateMachine() : base() { }

        public SubStateMachine(T entryStateName, params object[] parameters) : base(entryStateName, parameters) { }
        
        public bool CanEnter() {
            return true;
        }

        public void Start() {
            Transition(entryState);
        }

        public new E Update() {
            base.Update();
            return Name;
        }

        public void Exit() {
            currentState.Exit();
        }
    }
}