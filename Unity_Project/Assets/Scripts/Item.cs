using UnityEngine;

public abstract class Item {
    public string name;
    public int ID;
    public string description;
    public Sprite sprite;
    public bool stackable;
    public int stackSize;

    public Item(string name, int ID, string description, Sprite sprite, bool stackable, int stackSize) {
        this.name = name;
        this.ID = ID;
        this.description = description;
        this.sprite = sprite;
        this.stackable = stackable;
        this.stackSize = stackSize;
    }

    public abstract void Select(); //Left mouse click
    public abstract void Use(); //Right mouse click
}
