using TimefulDungeon.AI;
using TimefulDungeon.AI.AggroFSM;
using TimefulDungeon.Core;
using TimefulDungeon.Core.FiniteStateMachine;
using UnityEngine;

namespace TimefulDungeon.Enemies.Spoop {
    [RequireComponent(typeof(ContextSteering), typeof(WanderModule))]
    public class Spoop : Enemy {
        [SerializeField] private float meleeAttackRange = 0.75f;
        
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Dead = Animator.StringToHash("Dead");
        private static readonly int Speed = Animator.StringToHash("Speed");
        
        private FiniteStateMachine<AggroStates> _searchFsm;
        private Animator _animator;
        
        private bool _attacking;

        protected override void Start() {
            base.Start();
            _searchFsm = new FiniteStateMachine<AggroStates>(AggroStates.Wander, this);
            SetTarget(Player.instance.transform);
            _animator = GetComponent<Animator>();
        }

        protected override void Update() {
            base.Update();
            _searchFsm.Update();

            _animator.SetFloat(Speed, rigidbody.velocity.magnitude);
            var targetClose = transform.Position2D().CloserThan(target.Position2D(), meleeAttackRange);
            switch (_attacking) {
                case true when !targetClose:
                    _attacking = false;
                    _animator.SetBool(Attack, false);
                    RestoreSpeed();
                    break;
                case false when targetClose:
                    _attacking = true;
                    _animator.SetBool(Attack, true);
                    SetSpeed(0);
                    break;
            }
        }

        public override void Die() {
            _animator.SetTrigger(Dead);
            SetSpeed(0);
        }

        public void OnDeathAnimationComplete() {
            base.Die();
        }
    }
}