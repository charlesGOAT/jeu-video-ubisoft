using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static bool _isInstanceAssigned;

    [SerializeField] 
    public bool isSpreadingMode = false;
    
    public GridManagerStrategy GridManager { get; private set; }
    public BombManager BombManager { get; private set; }
    public ItemsManager ItemsManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public UIManager UIManager { get; private set; }

    
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

    public void RemoveItemFromGrid(Item item)
    {
        ItemsManager.RemoveItem(item.ItemType);
        GridManager.RemoveItemFromGrid(item);
    }

    private void GetManagers()
    {
        GridManager = FindFirstObjectByType<GridManagerStrategy>();
        BombManager = FindFirstObjectByType<BombManager>();
        ItemsManager = FindFirstObjectByType<ItemsManager>();
        ScoreManager = FindFirstObjectByType<ScoreManager>();
        UIManager = FindFirstObjectByType<UIManager>();

        if (GridManager == null)
        {
            throw new Exception("There's no active grid manager");
        }
        if (BombManager == null)
        {
            throw new Exception("There's no active bomb manager");
        }
        if (ItemsManager == null)
        {
            throw new Exception("There's no active items manager");
        }
        if (ScoreManager == null)
        {
            throw new Exception("There's no active score manager");
        }
        if (UIManager == null)
        {
            // throw new Exception("There's no active ui manager");
        }
        // add other managers
    }

    public void EndGame()
    {
        UIManager.endGameImage.gameObject.SetActive(true);
    }
}
