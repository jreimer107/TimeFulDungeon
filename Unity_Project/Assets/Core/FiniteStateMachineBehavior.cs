using System;
using UnityEngine;

namespace TimefulDungeon.Core {
    /// <summary>
    ///     MonoBehaviour wrapper around a finite state machine.
    ///     Must be overridden.
    /// </summary>
    /// <typeparam name="T">Type of enum to build states between.</typeparam>
    public abstract class FiniteStateMachineBehavior<T> : MonoBehaviour where T : Enum {
        private FiniteStateMachine<T> _that;
        public State<T> currentState => _that.currentState;
        
        protected virtual void Awake() {
            _that = new FiniteStateMachine<T>();
            _that.OnTransition += OnTransition;
        }

        protected virtual void Update() {
            _that.Update();
        }

        protected virtual void Initialize(T initialStateName) {
            _that.Initialize(initialStateName);
        }

        protected virtual void Transition(T toState) {
            _that.Transition(toState);
        }

        protected virtual void OnTransition() { }
    }
}