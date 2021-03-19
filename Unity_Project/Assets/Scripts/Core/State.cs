using System;
using System.Collections.Generic;

namespace TimefulDungeon.Core {
    public abstract class State<T> where T : Enum {
        public delegate bool TransitionCheck();
        protected readonly Dictionary<T, TransitionCheck> transitions;

        protected State() {
            transitions = new Dictionary<T, TransitionCheck>();
        }

        public T Name { get; protected set; }

        public bool HasTransition(T toState) {
            return transitions.ContainsKey(toState);
        }

        public TransitionCheck GetTransition(T toState) {
            transitions.TryGetValue(toState, out var transitionCheck);
            return transitionCheck;
        }

        public bool CanTransition(T toState) {
            var transitionCheck = GetTransition(toState);
            return transitionCheck != null && transitionCheck();
        }

        public virtual void Start() { }

        public virtual T Update() {
            return Name;
        }

        public virtual T FixedUpdate() {
            return Name;
        }

        public virtual T LateUpdate() {
            return Name;
        }

        public virtual void Exit() { }
    }
}