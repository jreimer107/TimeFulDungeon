using System;
using System.Collections.Generic;

namespace TimefulDungeon.Core {
    public abstract class State<T> where T : Enum {
        public delegate bool TransitionCheck();
        private readonly Dictionary<T, TransitionCheck> _transitions;

        protected State() {
            _transitions = new Dictionary<T, TransitionCheck>();
        }

        public T Name { get; protected set; }

        public bool HasTransition(T toState) {
            return _transitions.ContainsKey(toState);
        }

        protected void AddTransition(T toState, TransitionCheck checkFn = null) {
            _transitions.Add(toState, checkFn);
        }

        public TransitionCheck GetTransition(T toState) {
            _transitions.TryGetValue(toState, out var transitionCheck);
            return transitionCheck;
        }

        public bool CanTransition(T toState) {
            var transitionCheck = GetTransition(toState);
            return transitionCheck == null || transitionCheck();
        }

        public virtual void Start() { }

        public virtual T Update() {
            return Name;
        }

        public virtual void Exit() { }
    }
}