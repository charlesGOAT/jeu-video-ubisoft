using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField]
    private GameObject playerPrefab;
    private float _timeRemaining;
    private bool _timerRunning;

    public int CurrentMinutes => Mathf.FloorToInt(_timeRemaining / 60f);
    public int CurrentSeconds => Mathf.FloorToInt(_timeRemaining % 60f);
    
    [SerializeField] 
    private bool _isSpreadingMode = true;
    public bool IsSpreadingMode => _isSpreadingMode;
    
    public RuntimeConfigData RuntimeConfig { get; private set; }
    
    [SerializeField]
    private bool _isBonusSpeed = false;
    public bool IsBonusSpeed => _isBonusSpeed;

    public GridManagerStrategy GridManager { get; private set; }
    public BombManager BombManager { get; private set; }
    public ItemsManager ItemsManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public GameUIManager GameUIManager { get; private set; }
    public EventManager EventManager { get; private set; }

    // add other managers

    private void Update()
    {
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _timerRunning = false;
            GameUIManager.UpdateTimerDisplay();
            EndGame();
        }

        GameUIManager.UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        _timeRemaining = GameConstants.GAME_DURATION;
        _timerRunning = true;
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();

                if (_instance == null)
                {
                    _instance = CreateInstance();
                }
                _instance.GetManagers();
            }

            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        _instance = this;
        GetManagers();

#if !UNITY_EDITOR
    InitializeRuntimeConfig();
#endif
    }
    
    private static GameManager CreateInstance()
    {
        GameObject go = new GameObject($"{nameof(GameManager)} (Auto-Created)");
        var manager = go.AddComponent<GameManager>();

        return manager;
    }

    private void InitializeRuntimeConfig()
    {
        RuntimeConfig = RuntimeConfigLoader.GetConfig();
        _isSpreadingMode = RuntimeConfig.IsSpreadingMode;
        _isBonusSpeed = RuntimeConfig.IsBonusSpeed;
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
        EventManager = FindFirstObjectByType<EventManager>();

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
        if (EventManager == null)
        {
            throw new Exception("There's no active event manager");
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
        StartCoroutine(EndGameCoroutine());
    }

    private IEnumerator EndGameCoroutine()
    {
        Time.timeScale = 0f;
        GameUIManager.EndGame();
        CleanGame();
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void CleanGame()
    {
        Player.ActivePlayers.Clear();
    }
}
