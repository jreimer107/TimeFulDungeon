using System.Collections;
using TimefulDungeon.Enemies;
using UnityEngine;

namespace TimefulDungeon.AI.AggroFSM {
    public class SearchState : AggroStateBase {
        private bool _searching;
        private Coroutine _searchCoroutine;

        public SearchState(Enemy owner) : base(owner) {
            Name = AggroStates.Search;
        }

        public override void Start() {
            Debug.Log("Begin search");
            owner.WanderAround(owner.Position2D());
            _searchCoroutine = owner.StartCoroutine(SearchForSeconds());
        }

        public override AggroStates Update() {
            if (owner.CanSeeTarget()) {
                owner.StopCoroutine(_searchCoroutine);
                owner.StopWandering();
                return AggroStates.Attack;
            }
            
            if (!_searching) {
                return AggroStates.Wander;
            }
            
            return Name;
        }
        
        private IEnumerator SearchForSeconds() {
            _searching = true;
            yield return new WaitForSeconds(owner.searchTime);
            _searching = false;
        }
    }
}