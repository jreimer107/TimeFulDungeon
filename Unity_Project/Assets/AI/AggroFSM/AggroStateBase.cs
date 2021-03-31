using TimefulDungeon.Core;
using TimefulDungeon.Enemies;
using UnityEngine;

namespace TimefulDungeon.AI.AggroFSM {
    public abstract class AggroStateBase : State<AggroStates> {
        protected readonly Enemy owner;
        
        protected AggroStateBase(Enemy owner) {
            this.owner = owner;
        }
    }
}