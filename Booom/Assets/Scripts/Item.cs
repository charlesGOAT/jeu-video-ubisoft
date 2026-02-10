using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] 
    private ItemType itemType = ItemType.Basic;

    public ItemType ItemType => itemType;
    
    // Todo: faire classes enfants pour les differents types d'items
    
    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public enum ItemType
{
    Basic = 0 // todo add more
}
