using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static bool _isInstanceAssigned;

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField] 
    public bool _isSpreadingMode = true;
    public bool IsSpreadingMode => _isSpreadingMode;
    
    public RuntimeConfigData RuntimeConfig { get; private set; }
    
    public GridManagerStrategy GridManager { get; private set; }
    public BombManager BombManager { get; private set; }
    public ItemsManager ItemsManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public GameUIManager GameUIManager { get; private set; }

    // add other managers

    public static GameManager Instance
    {
        get
        {
            if (!_isInstanceAssigned)
            {
                var instance = FindFirstObjectByType<GameManager>() ?? AutoCreateInstance();
                SetSingletonInstance(instance);
                instance.GetManagers();
                
#if !UNITY_EDITOR
                instance.InitializeRuntimeConfig();
#endif
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

    private void InitializeRuntimeConfig()
    {
        RuntimeConfig = RuntimeConfigLoader.GetConfig();
        _isSpreadingMode = RuntimeConfig.IsSpreadingMode;
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
        GameUIManager = FindFirstObjectByType<GameUIManager>();

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
        if (GameUIManager == null)
        {
            throw new Exception("There's no active game ui manager");
        }
        // add other managers
    }

    public void SpawnPlayers()
    {
        foreach (var playerInput in LobbyManager.JoinedPlayers) 
        {
            Vector2Int spawnPoint = GridManager.playerSpawnPoints[playerInput.playerIndex];
            spawnPoint *= GameConstants.UNITY_GRID_SIZE;
            
            PlayerInput newInput = PlayerInput.Instantiate(playerPrefab, playerIndex:playerInput.playerIndex, pairWithDevices:playerInput.devices.ToArray());
            newInput.transform.position = new Vector3(spawnPoint.x, 2.0f, spawnPoint.y);
            
            Destroy(playerInput.gameObject); //Destroying dummy prefabs
        }
        
        LobbyManager.JoinedPlayers.Clear();
    }

    public void EndGame()
    {
        GameUIManager.endGameImage.gameObject.SetActive(true);
    }
}
