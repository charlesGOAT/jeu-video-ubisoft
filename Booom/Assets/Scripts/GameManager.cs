using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField]
    private GameObject playerPrefab;
    public float TimeRemaining { get; private set; }
    private bool _timerRunning;
    
    [SerializeField]
    private float _gameDuration = GameConstants.GAME_DURATION;

    [SerializeField] 
    public Material snowflakeMaterial;
    [SerializeField] 
    public Material transparentMat;
    [SerializeField] 
    public Material highlightMat;
    [SerializeField] 
    public Material blinkMat;

    [SerializeField] 
    public Material paintBrushEffect;

    public float  GameDuration => _gameDuration;
    public int CurrentMinutes => Mathf.FloorToInt(TimeRemaining / 60f);
    public int CurrentSeconds => Mathf.FloorToInt(TimeRemaining % 60f);
    
    [SerializeField] 
    private bool _isSpreadingMode = true;
    public bool IsSpreadingMode => _isSpreadingMode;
    
    public RuntimeConfigData RuntimeConfig { get; private set; }
    
    [SerializeField]
    private bool _isBonusSpeed = true;

    [SerializeField] 
    public int FrozenTileDuration = 15;

    public bool IsBonusSpeed => _isBonusSpeed;

    public GridManagerStrategy GridManager { get; private set; }
    public BombManager BombManager { get; private set; }
    public ItemsManager ItemsManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public GameUIManager GameUIManager { get; private set; }
    public EventManager EventManager { get; private set; }

    public readonly int[] CollisionLayers = new int[GameConstants.NB_PLAYERS] { 8, 9, 10, 11 };

    public float ColorDebuff { get; private set; } = GameConstants.COLOR_DEBUFF;
    public float ColorBoost { get; private set; } = GameConstants.COLOR_BOOST;
    public bool HighlightOwnColor { get; private set; }
    public bool HasRoundEnded { get; private set; }

    private bool _hasChangedForFastMusic = false;

    // add other managers
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>() ?? CreateInstance();
                _instance.GetManagers(); 
                
#if !UNITY_EDITOR
                _instance.InitializeRuntimeConfig();
#endif
            }

            return _instance;
        }
    }

    private void Update()
    {
        if (!_timerRunning) return;

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            _timerRunning = false;
            GameUIManager.UpdateTimerDisplay();
            EndGame();
        }

        GameUIManager.UpdateTimerDisplay();
        
        UpdateMusic();
    }

    private void UpdateMusic()
    {
        if (TimeRemaining <= 30 && !_hasChangedForFastMusic)
        {
            SoundManager.Instance.OnPlayAcceleratedGameMusic();
            _hasChangedForFastMusic = true;
        }
    }

    public void StartTimer()
    {
        SoundManager.Instance.OnGameStarted();
        TimeRemaining = _gameDuration;
        _timerRunning = true;
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        
        _instance = this;
        GetManagers();

#if !UNITY_EDITOR
    InitializeRuntimeConfig();
#endif
    }
    private static GameManager CreateInstance() => new GameObject($"{nameof(GameManager)} (Auto-Created)",
        typeof(GameManager)).GetComponent<GameManager>();

    private void InitializeRuntimeConfig()
    {
        RuntimeConfig = RuntimeConfigLoader.GetConfig();
        _isSpreadingMode = RuntimeConfig.IsSpreadingMode;
        _isBonusSpeed = RuntimeConfig.IsBonusSpeed;
        _gameDuration =  RuntimeConfig.GameDuration;
        FrozenTileDuration = RuntimeConfig.FrozenTileDuration;
        ColorBoost = RuntimeConfig.ColorBoost;
        ColorDebuff = RuntimeConfig.ColorDebuff;
        HighlightOwnColor = RuntimeConfig.HighlightOwnColor;
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
        if (snowflakeMaterial == null)
        {
            throw new Exception("Snowflake material cannot be null");
        }
        if (transparentMat == null)
        {
            throw new Exception("Transparent material cannot be null");
        }
        if (paintBrushEffect == null)
        {
            throw new Exception("Paintbrush effect cannot be null");
        }
        // add other managers
    }

    public void SpawnPlayers()
    {
        var playersToSpawn = LobbyManager.JoinedPlayers.Values.ToList();
        
        foreach (var playerInput in playersToSpawn)
        {
            Vector2Int spawnPoint = GridManager.playerSpawnPoints[playerInput.playerIndex];
            Vector3 worldPos = GridManagerStrategy.GridToWorldPosition(spawnPoint);

            playerPrefab.layer = CollisionLayers[playerInput.playerIndex];
            PlayerInput newInput = PlayerInput.Instantiate(playerPrefab, playerIndex:playerInput.playerIndex, pairWithDevices:playerInput.devices.ToArray());
            newInput.transform.position = new Vector3(worldPos.x, 0.0f, worldPos.z);
        }
    }

    private void DestroyAllItems()
    {
        var items = GameObject.FindGameObjectsWithTag("Item");
        foreach (var item in items)
        {
            Destroy(item);
        }
    }

    private void RemoveAllItemsInPlayerInv()
    {
        foreach (Player player in Player.ActivePlayers.Values)
        {
            player.ResetInventory();
        }
    }
    
    public void EndGame()
    {
        PlayerEnum winner = ScoreManager.FindPlayerWithMostGround();
        Bomb.ActiveBombsGO.ForEach(Destroy);
        HasRoundEnded = true;
        
        DestroyAllItems();
        RemoveAllItemsInPlayerInv();
        
        if (RoundManager.ShouldEndGame(winner))
        {
            StartCoroutine(EndGameCoroutine(winner));
        }
        else
        {
            StartCoroutine(EndRoundCoroutine(winner));
        }
    }

    public IEnumerator MakeWinnerColorBlink(PlayerEnum winner)
    {
        var acquiredTiles = ScoreManager.AcquiredTilesByPlayer[winner];
        foreach (var pos in acquiredTiles)
        {
            Tile tile = GridManager.GetTileAtCoordinates(pos);
            if (tile == null) continue;
            
            tile.AddWinnerBlink();
        }

        yield return new WaitForSeconds(4f); // todo tweak
    }

    private IEnumerator EndRoundCoroutine(PlayerEnum winner)
    {
        Player.ActivePlayers.Values.ToList().ForEach(x => x.DisableInputActions());
        RoundManager.LoadEndRoundData();
        SoundManager.Instance.OnColorAlternate();
        yield return StartCoroutine(MakeWinnerColorBlink(winner));
        Player.ActivePlayers.Values.ToList().ForEach(x => x.EnableInputActions());
        NewRound();
        SceneManager.LoadScene("EndGame");
    }

    private IEnumerator EndGameCoroutine(PlayerEnum winner)
    {
        Player.ActivePlayers.Values.ToList().ForEach(x => x.DisableInputActions());
        RoundManager.LoadEndGameData();
        SoundManager.Instance.OnColorAlternate();
        yield return StartCoroutine(MakeWinnerColorBlink(winner));
        Player.ActivePlayers.Values.ToList().ForEach(x => x.EnableInputActions());
        CleanGame();
        SceneManager.LoadScene("EndGame");
    }

    private void CleanGame()
    {
        Player.ActivePlayers.Clear();
        Bomb.ActiveBombs.Clear();
        RoundManager.CleanGame();
    }

    private void NewRound()
    {
        Bomb.ActiveBombs.Clear();
        Player.ActivePlayers.Clear();
    }
}
