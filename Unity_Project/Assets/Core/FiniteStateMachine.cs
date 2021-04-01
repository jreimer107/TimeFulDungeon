using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TimefulDungeon.Core {
    public class FiniteStateMachine<T> where T : Enum {
        public bool isInitialized;
        protected readonly Dictionary<T, State<T>> states;
        protected readonly T entryState;

        /// <summary>
        ///     Called after calling Start on the new state. Use to change the data of the
        ///     inheriting class to prepare for the new state.
        /// </summary>
        public event Action OnTransition;

        public State<T> currentState { get; private set; }

        public void Update() {
            var nextState = currentState.Update();
            if (!Equals(nextState, currentState.Name)) Transition(nextState);
        }
        
        public FiniteStateMachine() {
            states = new Dictionary<T, State<T>>();
        }
        
        public FiniteStateMachine(T entryStateName, params object[] parameters) {
            isInitialized = true;
            entryState = entryStateName;
            states = new Dictionary<T, State<T>>();

            var stateClasses = Assembly.GetAssembly(typeof(State<T>)).GetTypes().Where(x =>
                x.IsSubclassOf(typeof(State<T>)) &&
                x.IsClass &&
                !x.IsAbstract
            );

            foreach (var stateClass in stateClasses) {
                var instance = (State<T>) Activator.CreateInstance(stateClass, parameters);
                states.Add(instance.Name, instance);
            }
            
            Transition(entryState);
            if (currentState == null) {
                Debug.LogError($"FSM on {GetType()} was passed an initial state that does not exist.");
            }
        }

        public void Transition(T toState) {
            if (!isInitialized) return;
            var newState = states[toState]; 
            if (!newState.CanEnter()) return;
            currentState?.Exit();
            currentState = newState;
            currentState.Start();
            OnTransition?.Invoke();
        }
    }
}