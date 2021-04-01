using System;

namespace TimefulDungeon.Core.FiniteStateMachine {
    public abstract class State<T> : IState<T> where T : Enum {
        public T Name { get; protected set; }
        
        public virtual bool CanEnter() {
            return true;
        }

        public virtual void Start() { }


        public virtual T Update() {
            return Name;
        }

        public virtual void Exit() { }
    }
}