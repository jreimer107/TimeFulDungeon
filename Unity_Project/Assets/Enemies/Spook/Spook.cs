using TimefulDungeon.AI;
using TimefulDungeon.AI.AggroFSM;
using TimefulDungeon.Core;
using TimefulDungeon.Core.FiniteStateMachine;
using UnityEngine;

namespace TimefulDungeon.Enemies.Spook {
    [RequireComponent(typeof(ContextSteering), typeof(WanderModule))]
    public class Spook : Enemy {
        [SerializeField] private float meleeAttackRange = 3;
        [SerializeField] private float meleeAttackCooldown = 0.5f;
        [SerializeField] private float rangedAttackRange = 10;
        [SerializeField] private float rangedAttackCooldown = 5;
        
        private static readonly int MeleeAttack = Animator.StringToHash("Attack1");
        private static readonly int RangedAttack = Animator.StringToHash("Attack2");
        private static readonly int Dead = Animator.StringToHash("Dead");
        private static readonly int Speed = Animator.StringToHash("Speed");
        
        private FiniteStateMachine<AggroStates> _searchFsm;
        private Animator _animator;
        
        private bool _attacking;
        private bool _meleeAttackUsed;
        private bool _rangedAttackUsed;

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
            Attack();
        }

        private void Attack() {
            if (_attacking) return;
            var squaredTargetDistance = (target.Position2D() - transform.Position2D()).sqrMagnitude;
            if (!_rangedAttackUsed && squaredTargetDistance <= Mathf.Pow(rangedAttackRange, 2)) {
                _rangedAttackUsed = true;
                Invoke(nameof(RestoreRangedAttack), rangedAttackCooldown);
                _animator.SetTrigger(RangedAttack);
                SetSpeed(0);
            }
            else if (!_meleeAttackUsed && squaredTargetDistance <= Mathf.Pow(meleeAttackRange, 2)) {
                _meleeAttackUsed = true;
                Invoke(nameof(RestoreMeleeAttack), meleeAttackCooldown);
                _animator.SetTrigger(MeleeAttack);
                SetSpeed(0);
            }
        }

        public override void Die() {
            _animator.SetTrigger(Dead);
            SetSpeed(0);
        }
        
        public void OnDeathAnimationComplete() {
            base.Die();
        }

        public void SetAttacking() {
            _attacking = true;
        }

        public void UnsetAttacking() {
            _attacking = false;
            RestoreSpeed();
        }
        
        private void RestoreMeleeAttack() => _meleeAttackUsed = false;
        private void RestoreRangedAttack() => _rangedAttackUsed = false;
    }
}