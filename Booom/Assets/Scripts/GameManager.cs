using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [SerializeField]
    private float _gameDuration = GameConstants.GAME_DURATION;

    [SerializeField] 
    public Material snowflakeMaterial;
    [SerializeField] 
    public Material transparentMat;

    [SerializeField] 
    public Material paintBrushEffect;

    [SerializeField] 
    public Material highlightMat;
    
    [SerializeField]
    private List<int> mapsToPlay = new ();
    private readonly List<int> _mapsPlayed = new();

    private bool _isRandomMap = true;
    private int _lastMapIndex = 0;

    private static Dictionary<PlayerEnum, int> _playerWinsDict = new()
    {
        { PlayerEnum.Player1, 0 },
        { PlayerEnum.Player2, 0 },
        { PlayerEnum.Player3, 0 },
        { PlayerEnum.Player4, 0 },
    };

    public float  GameDuration => _gameDuration;
    public int CurrentMinutes => Mathf.FloorToInt(_timeRemaining / 60f);
    public int CurrentSeconds => Mathf.FloorToInt(_timeRemaining % 60f);
    
    [SerializeField] 
    private bool _isSpreadingMode = true;
    public bool IsSpreadingMode => _isSpreadingMode;
    
    public RuntimeConfigData RuntimeConfig { get; private set; }
    
    [SerializeField]
    private bool _isBonusSpeed = false;

    [SerializeField] 
    public int FrozenTileDuration = 30;

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

    private bool _hasChangedForFastMusic = false;

    private static readonly List<PlayerEnum> _gameWonPlayer = new();
    
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

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _timerRunning = false;
            GameUIManager.UpdateTimerDisplay();
            EndGame();
        }

        GameUIManager.UpdateTimerDisplay();
        
        UpdateMusic();
    }

    private void UpdateMusic()
    {
        if (_timeRemaining <= 60 && _hasChangedForFastMusic)
        {
            SoundManager.Instance.OnPlayAcceleratedGameMusic();
            _hasChangedForFastMusic = true;
        }
    }

    public void StartTimer()
    {
        SoundManager.Instance.OnGameStarted();
        _timeRemaining = _gameDuration;
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
        _isRandomMap = RuntimeConfig.IsRandomMap;
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

    public void EndGame()
    {
        PlayerEnum winner = ScoreManager.FindPlayerWithMostGround();
        
        if (ShouldEndGame(winner))
        {
            StartCoroutine(EndGameCoroutine(winner));
            SoundManager.Instance.OnGameEnded();
        }
        else
        {
            StartCoroutine(EndRoundCoroutine(winner));
        }
        
        //a fix plus tard quand la fin de la game arrive
    }

    private IEnumerator EndRoundCoroutine(PlayerEnum winner)
    {
        Time.timeScale = 0f;
        // faire glow la couleur du winner
        // faire jouer la musique de fin de round
        NewRound();
        yield return new WaitForSecondsRealtime(5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(FindNextMap());
    }

    private int FindNextMap()
    {
        int newMapIndex = -1;

        if (_isRandomMap)
        {
            System.Random rand = new();
            int count = 0;
            do
            {
                newMapIndex = rand.Next(0, mapsToPlay.Count);

                if (count++ >= mapsToPlay.Count)
                {
                    Debug.LogError("There's not enough maps, playing already played random map");
                    break;
                }
            } 
            while (_mapsPlayed.Contains(mapsToPlay[newMapIndex]));
        }
        else
        {
            newMapIndex = ++_lastMapIndex;
        }

        int nextMap = mapsToPlay[newMapIndex];
        _mapsPlayed.Add(nextMap);
        return nextMap;
    }

    private IEnumerator EndGameCoroutine(PlayerEnum winner)
    {
        Time.timeScale = 0f;
        
        // faire glow la couleur du winner
        // faire jouer la musique de fin de round
        LoadEndGameData();
        CleanGame();
        yield return new WaitForSecondsRealtime(5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("EndGame"); // EndGameMenu
    }

    private void CleanGame()
    {
        Player.ActivePlayers.Clear();
        Bomb.ActiveBombs.Clear();
        Bomb.ActiveBombsGO.ForEach(Destroy);
        _playerWinsDict.Clear();
        foreach (PlayerEnum player in Enum.GetValues(typeof(PlayerEnum)))
        {
            _playerWinsDict[player] = 0;
        }
        _gameWonPlayer.Clear();
    }

    private void LoadEndGameData()
    {
        var playerRanks = _playerWinsDict
            .OrderByDescending(x => x.Value)
            .Select((x, index) => (Player: x.Key, Rank: index))
            .ToDictionary(item => item.Player, item => item.Rank);
        
        foreach (Player player in Player.ActivePlayers)
        {
            if (player.PlayerNb == PlayerEnum.None) continue;
            EndGameUIManager.PlayerRank[player.PlayerNb] = playerRanks[player.PlayerNb];
        }

        EndGameUIManager.PlayerWonGame = new(_gameWonPlayer);
    }

    private bool ShouldEndGame(in PlayerEnum winner)
    {
        _playerWinsDict[winner]++;
        _gameWonPlayer.Add(winner);
        return _playerWinsDict[winner] == 2;
    }

    private void NewRound()
    {
        Bomb.ActiveBombs.Clear();
        Player.ActivePlayers.Clear();
    }
}
