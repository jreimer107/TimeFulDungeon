using System;
using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Item", menuName = "Interactables/Item")]
    [Serializable]
    public class ItemTemplate : ScriptableObject {
        public new string name;
        public int id;
        public string description;
        public string redText;
        public bool stackable;
        public int count;
        public float cooldown;
        public bool autoPickup;
        
        // Unity fields, not serializable
        public Sprite sprite;
        public AnimationClip idleClip;
        public AnimationClip actionClip;
        public AudioClip soundEffect;

        public virtual Item GetInstance() {
            return new Item(this);
        }
    }
}