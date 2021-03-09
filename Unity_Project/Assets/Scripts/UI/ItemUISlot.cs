using TimefulDungeon.Core;
using TimefulDungeon.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoraUtils;

namespace TimefulDungeon.UI {
    [RequireComponent(typeof(Button))]
    public class ItemUISlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler,
        IDropHandler, IPointerEnterHandler, IPointerExitHandler {
        private static Tooltip tooltip;
        public Item item;
        [SerializeField] private Image icon;
        private Button button;

        public bool IsEmpty => !item;

        private void Awake() {
            button = GetComponent<Button>();
        }

        protected virtual void Start() {
            tooltip ??= Tooltip.instance;
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (IsEmpty || eventData.button != PointerEventData.InputButton.Left) return;
            Debug.Log("Left Click drag.");
            ClickAndDrag.instance.SetHeldItem(item);
            icon.color = Color.gray;
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnDrop(PointerEventData eventData) {
            Debug.Log("Dropped!");
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (IsEmpty || eventData.button != PointerEventData.InputButton.Left) return;
            var droppedLocation = eventData.pointerCurrentRaycast.gameObject;
            if (droppedLocation != null) {
                var droppedSlot = droppedLocation.GetComponent<ItemUISlot>();
                if (droppedSlot != null) {
                    Debug.Log("Dropped item on other slot.");
                    DropOn(droppedSlot);
                }
                else {
                    Debug.Log("Dropped item in UI area.");
                }
            }
            else {
                Debug.Log("Dropped item to discard.");
                DiscardItem();
            }

            ClickAndDrag.instance.UnsetHeldItem();
            // icon.enabled = true;
            icon.color = Color.white;
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (IsEmpty || eventData.button != PointerEventData.InputButton.Right) return;
            Debug.Log("Right click");
            Use();
        }

        //This method should ONLY be called by a UI manager script.
        //If you want to edit the contents of this slot, go through a UI manager.
        public virtual void Refresh() {
            if (!item) {
                icon.enabled = false;
                button.interactable = false;
                return;
            }

            //Set up slot to render sprite
            icon.sprite = item.sprite;
            icon.preserveAspect = true;
            icon.enabled = true;

            //Allow the button to be clickable
            button.interactable = true;
        }

        protected virtual void Use() {
            item.Use();
        }

        protected virtual void DiscardItem() {
            var mouseDistance = Utils.GetMouseWorldPosition2D() - Player.instance.transform.Position2D();
            var pushVelocity = mouseDistance * 5;
            Pickup.Create(item, Player.instance.transform.Position2D(), pushVelocity);
        }

        protected virtual void DropOn(ItemUISlot otherSlot) { }

        #region TooltipHover

        public void OnPointerEnter(PointerEventData eventData) {
            if (item) tooltip.ShowTextOnDelay(item.GetTooltipText(), 300);
        }

        public void OnPointerExit(PointerEventData eventData) {
            tooltip.Hide();
        }

        #endregion
    }
}