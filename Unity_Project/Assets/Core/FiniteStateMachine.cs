using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TimefulDungeon.Core {
    public abstract class FiniteStateMachine<T> : MonoBehaviour where T : Enum {
        protected State<T> currentState;
        private bool _isInitialized;

        private Dictionary<T, State<T>> _states;

        protected virtual void Update() {
            var nextState = currentState.Update();
            if (!Equals(nextState, currentState.Name)) Transition(nextState);
        }

        protected virtual void Initialize(T initialStateName) {
            if (_isInitialized)
                Debug.LogError($"The FiniteStateMachine component on {GetType()} is already initialized.");

            _isInitialized = true;

            _states = new Dictionary<T, State<T>>();
            var stateClasses = Assembly.GetAssembly(typeof(State<T>)).GetTypes().Where(x =>
                x.IsSubclassOf(typeof(State<T>)) &&
                x.IsClass &&
                !x.IsAbstract
            );

            var parameters = new object[] {this};
            foreach (var stateClass in stateClasses) {
                var instance = (State<T>) Activator.CreateInstance(stateClass, parameters);
                _states.Add(instance.Name, instance);
            }

            var initialState = _states[initialStateName];
            if (initialState != null) {
                currentState = initialState;
                currentState.Start();
            }
            else {
                Debug.LogError($"FSM on {GetType()} was passed an initial state that does not exist.");
            }
        }

        protected void Transition(T toState) {
            if (!_isInitialized) return;
            if (!Equals(currentState.Name, toState) && !currentState.CanTransition(toState)) return;
            currentState.Exit();
            currentState = _states[toState];
            currentState.Start();
            OnTransition();
        }
        
        /// <summary>
        /// Called after calling Start on the new state. Use to change the data of the
        /// inheriting class to prepare for the new state.
        /// </summary>
        protected virtual void OnTransition() { }
    }
}