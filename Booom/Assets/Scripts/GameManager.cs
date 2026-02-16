using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static bool _isInstanceAssigned;

    [SerializeField] 
    public bool isSpreadingMode = true;
    
    public GridManagerStategy GridManager { get; private set; }
    public BombManager BombManager { get; private set; }
    // add other managers

    public static GameManager Instance
    {
        get
        {
            if (!_isInstanceAssigned)
            {
                var instance = AutoCreateInstance();
                SetSingletonInstance(instance);
                instance.GetManagers();
            }

            return _instance;
        }
    }

    private static GameManager AutoCreateInstance() =>
        new GameObject($"{nameof(GameManager)} (Auto-Created)", typeof(GameManager)).GetComponent<GameManager>();
    
    private static void SetSingletonInstance(GameManager instance)
    {
        if (instance == null)
            throw new ArgumentNullException("instance must not be null");

        _instance = instance;
        _isInstanceAssigned = true;
    }

    public void RemoveItemFromGrid(ItemType itemType)
    {
        // todo 
    }

    private void GetManagers()
    {
        GridManager = FindFirstObjectByType<GridManagerStategy>();
        BombManager = FindFirstObjectByType<BombManager>();

        if (GridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
        if (BombManager == null)
        {
            throw new Exception("There's no active bomb manager");
        }
        // add other managers
    }
}
