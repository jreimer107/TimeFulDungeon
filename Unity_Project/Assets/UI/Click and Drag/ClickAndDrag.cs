using TimefulDungeon.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TimefulDungeon.UI {
    [RequireComponent(typeof(Image))]
    public class ClickAndDrag : MonoBehaviour {
        private Image icon;
        private Item item;

        public bool Active => item != null;

        private void Start() {
            icon = GetComponent<Image>();
        }

        // Update is called once per frame
        private void Update() {
            icon.transform.position = Input.mousePosition;
        }

        public void SetHeldItem(Item newItem) {
            item = newItem;
            icon.enabled = true;
            icon.sprite = newItem.Sprite;
            icon.preserveAspect = true;
        }

        public void UnsetHeldItem() {
            item = null;
            icon.enabled = false;
        }

        #region Singleton

        public static ClickAndDrag instance;
        
        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of ClickAndDrag found.");
            instance = this;
        }
        
        #endregion
    }
}