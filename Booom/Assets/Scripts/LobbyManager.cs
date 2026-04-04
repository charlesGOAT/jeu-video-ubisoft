using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playButton;
    
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField] 
    private MenuUIManager menuUIManager;

    [SerializeField]
    private GameObject[] playerLights;

    public static LobbyManager Instance { get; private set; }

    public static readonly Dictionary<PlayerInput, float> JoinTimes = new();
    public static readonly Dictionary<PlayerEnum, PlayerInput> JoinedPlayers = new ();

    public static bool ItemsActivated = true;
    public static bool TutorialActivated = true;
    public static bool TokebaqueIcitte = false;
    public static bool CVDActivated;
    public static int CVDIndex = 0;

    private PlayerInputManager _inputManager;

    private InputSystemUIInputModule[] _uiInputs;

    private static Dictionary<PlayerEnum, Vector3> _menuSpawnPoints = new();
    private static bool _playersDisappeared;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _inputManager = GetComponent<PlayerInputManager>();
        _inputManager.DisableJoining();
    }

    private void OnEnable()
    {
        _inputManager.onPlayerJoined += OnPlayerJoined;
        _inputManager.onPlayerLeft += OnPlayerLeft;
        SceneManager.sceneLoaded += OnSceneLoaded;

        InputUser.onChange += OnInputUserChange;
    }

    private void OnDisable()
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined;
        _inputManager.onPlayerLeft -= OnPlayerLeft;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        InputUser.onChange -= OnInputUserChange;
    }

    private void Start()
    {
       _uiInputs = FindObjectsByType<InputSystemUIInputModule>(FindObjectsInactive.Include, FindObjectsSortMode.None);
       _inputManager.EnableJoining();
       
       if (SceneManager.GetActiveScene().name == "Menu")
       {
           foreach (var player in JoinedPlayers)
           {
               player.Value.DeactivateInput();
               player.Value.gameObject.SetActive(true);
           }
           if (JoinedPlayers.Count != 0)
           {
               GiveUIControl(JoinedPlayers.First().Value);
           }
       }
    }

    public void GameStarted(int levelIndex)
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined; //Cannot join mid game
        GameManager.Instance.CleanGame();
        
        foreach (PlayerInput playerInput in JoinedPlayers.Values)
        {
            playerInput.SwitchCurrentActionMap("Player");
            playerInput.ActivateInput();
        }
        
        SceneManager.LoadScene(levelIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            StartCoroutine(AppearPlayers());
        }
        else if (scene.name != "EndGame")
        {
            DisappearPlayers();
            GameManager.Instance.SpawnPlayers();
        }
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            throw new Exception("No active Player Input Leaving");
        }
        if (!JoinedPlayers.Values.Contains(playerInput)) return;
        
        int leavingIndex = playerInput.playerIndex;
        PlayerEnum leavingPlayerEnum = (PlayerEnum) leavingIndex + 1;
        JoinedPlayers.Remove(leavingPlayerEnum);

        if (playerInput.inputIsActive && JoinedPlayers.Count > 0)
        {
            PlayerInput newUI = JoinedPlayers.First().Value;
            GiveUIControl(newUI);
        }
        
        if (SceneManager.GetActiveScene().name == "Menu")
            PlayerMenuLeft(leavingIndex);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            throw new Exception("No active Player Input Joining");
        }
        int intPlayerEnum = playerInput.playerIndex + 1;
        PlayerEnum playerEnum = (PlayerEnum) intPlayerEnum;

        float yRotation = 0f;

        switch (playerEnum)
        {
            case PlayerEnum.Player1:
                Player.PlayerColorDict[playerEnum] = new Color(255f / 255f, 41f / 255f, 117f / 255f);
                yRotation = -125f;
                break;
            case PlayerEnum.Player2:
                Player.PlayerColorDict[playerEnum] = new Color(0f, 245f / 255f, 212f / 255f);
                yRotation = -105f;
                break;
            case PlayerEnum.Player3:
                Player.PlayerColorDict[playerEnum] = new Color(107f / 255f, 44f / 255f, 255f / 255f);
                yRotation = -75f;
                break;
            case PlayerEnum.Player4:
                Player.PlayerColorDict[playerEnum] = new Color(255f / 255f, 255f / 255f, 33f / 255f);
                yRotation = -55f;
                break;
            default:
                Debug.LogWarning("Maximum of " + GameConstants.NB_PLAYERS + " players reached. Extra device ignored.");
                return;
        }

        DontDestroyOnLoad(playerInput.gameObject);
        JoinedPlayers[playerEnum] = playerInput;
        JoinTimes[playerInput] = Time.time;

        if (JoinedPlayers.Count == 1)
        {
            GiveUIControl(playerInput, true);
        }
        else
        {
            playerInput.DeactivateInput();
        }

        if (SceneManager.GetActiveScene().name == "Menu")
            SpawnMenuPlayer(playerInput, yRotation);
    }
    private void OnInputUserChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (SceneManager.GetActiveScene().name != "Menu") return;
        
        if (change == InputUserChange.DeviceLost)
        {
            PlayerInput pi = PlayerInput.all.FirstOrDefault(p => p.user == user);
            if (pi != null)
            {
                Destroy(pi.gameObject, 0.1f);
            }
        }
    }

    private void GiveUIControl(PlayerInput playerInput, bool firstPlayer = false)
    {
        playerInput.SwitchCurrentActionMap("UI");
        playerInput.ActivateInput();

        foreach (var uiInput in _uiInputs)
        {
            if (uiInput == null) return;
            uiInput.actionsAsset = playerInput.actions;
        }
        
        if (firstPlayer)
            EventSystem.current.SetSelectedGameObject(playButton);
    }

    private void DisappearPlayers()
    {
        if (_playersDisappeared) return;
        foreach (var player in JoinedPlayers)
        {
            _menuSpawnPoints[player.Key] = player.Value.gameObject.transform.position;
            player.Value.transform.position = new Vector3(999,0,999);
        }

        _playersDisappeared = true;
    }
    
    private IEnumerator AppearPlayers()
    {
        yield return null;
        foreach (var player in JoinedPlayers)
        {
            var p = player.Value.GetComponent<Player>();
            Player.ActivePlayers[player.Key] = p;
            player.Value.GetComponent<Player>().InitializeSpawner();
        }
        _playersDisappeared = false;
    }

    private void SpawnMenuPlayer(in PlayerInput playerInput, in float yRotation)
    {
        playerPrefab.layer = GameManager.Instance.CollisionLayers[playerInput.playerIndex];
        playerInput.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerLights[playerInput.playerIndex].SetActive(true);
    }

    private void PlayerMenuLeft(in int leavingIndex)
    {
        Vector2Int spawnPoint = GameManager.Instance.GridManager.playerSpawnPoints[leavingIndex];
        Tile tile = GameManager.Instance.GridManager.GetTileAtCoordinates(spawnPoint);
        tile.IsSpawn = false;
        tile.ChangeTileColor(PlayerEnum.None);
        playerLights[leavingIndex].SetActive(false);
    }
}
