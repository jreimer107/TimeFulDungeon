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
        

        protected virtual void Update() {
            _that?.Update();
        }

        protected virtual void Initialize(T initialStateName, params object[] parameters) {
            if (_that.isInitialized) {
                Debug.LogError($"The FiniteStateMachine component on {GetType()} is already initialized.");
                return;
            }
            
            _that = new FiniteStateMachine<T>(initialStateName, parameters);
            _that.OnTransition += OnTransition;
        }

        protected virtual void Transition(T toState) {
            _that?.Transition(toState);
        }

        protected virtual void OnTransition() { }
    }
}