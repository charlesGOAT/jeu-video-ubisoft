using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    private readonly Dictionary<ItemType, Item> _itemsInventory = new();

    public void AddNewItem(Item item)
    {
        if (!_itemsInventory.ContainsKey(item.ItemType))
            _itemsInventory[item.ItemType] = (Item)item.Clone();  // copy item because it will be destroyed
    }

}
