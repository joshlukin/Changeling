using UnityEngine;
 
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "Unnamed Item";
    public string description = "";
    public Sprite icon; // optional, for UI later
}