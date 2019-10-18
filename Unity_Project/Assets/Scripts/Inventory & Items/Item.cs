using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Interactables/Item")]
public class Item : ScriptableObject {
    new public string name;
    public int ID;
    public string description;
    public Sprite sprite;
    public bool stackable;
    public int count;

    public Item(string name, int ID, string description, Sprite sprite, bool stackable, int count) {
        this.name = name;
        this.ID = ID;
        this.description = description;
        this.sprite = sprite;
        this.stackable = stackable;
        this.count = count;
    }

    public void Select() { } //Left mouse click
    public void Use() { } //Right mouse click
}
