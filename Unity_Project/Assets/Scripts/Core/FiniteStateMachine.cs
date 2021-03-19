using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TimefulDungeon.Core {
    public abstract class FiniteStateMachine<T> : MonoBehaviour where T : Enum {
        protected State<T> currentState;
        private bool _isInitialized;

        protected Dictionary<T, State<T>> states;

        protected virtual void Update() {
            var nextState = currentState.Update();
            if (!Equals(nextState, currentState.Name)) Transition(nextState);
        }

        protected virtual void FixedUpdate() {
            var nextState = currentState.FixedUpdate();
            if (!Equals(nextState, currentState.Name)) Transition(nextState);
        }

        protected virtual void LateUpdate() {
            var nextState = currentState.LateUpdate();
            if (!Equals(nextState, currentState.Name)) Transition(nextState);
        }

        public virtual void Initialize(T initialStateName) {
            if (_isInitialized)
                Debug.LogError($"The FiniteStateMachine component on {GetType()} is already initialized.");

            _isInitialized = true;

            states = new Dictionary<T, State<T>>();
            var stateClasses = Assembly.GetAssembly(typeof(State<T>)).GetTypes().Where(x =>
                x.IsSubclassOf(typeof(State<T>)) &&
                x.IsClass &&
                !x.IsAbstract
            );

            var parameters = new object[] {this};
            foreach (var stateClass in stateClasses) {
                var instance = (State<T>) Activator.CreateInstance(stateClass, parameters);
                states.Add(instance.Name, instance);
            }

            var initialState = states[initialStateName];
            if (initialState != null) {
                currentState = initialState;
                currentState.Start();
            }
            else {
                Debug.LogError($"FSM on {GetType()} was passed an initial state that does not exist.");
            }
        }

        protected bool CanTransition(T toState) {
            if (!_isInitialized) return false;
            return states.ContainsKey(toState) && currentState.HasTransition(toState);
        }

        public virtual bool Transition(T toState) {
            if (!_isInitialized) return false;
            if (!currentState.CanTransition(toState)) return false;
            currentState.Exit();
            currentState = states[toState];
            currentState.Start();
            return true;
        }

        
    }
}