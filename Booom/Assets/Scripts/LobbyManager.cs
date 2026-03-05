using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public delegate void LobbyPlayerCountChanged(int playerCount);

public class LobbyManager : MonoBehaviour
{
    [SerializeField] 
    private GameObject _menuPlayerPrefab;
    
    public static LobbyManager Instance { get; private set; }
    public event LobbyPlayerCountChanged OnLobbyPlayerCountChanged;
    
    public static readonly Dictionary<PlayerEnum, PlayerInput> JoinedPlayers = new ();
    private PlayerInputManager _inputManager;
    
    private InputSystemUIInputModule[] _uiInputs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _inputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        _inputManager.onPlayerJoined += OnPlayerJoined;
        _inputManager.onPlayerLeft += OnPlayerLeft;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined;
        _inputManager.onPlayerLeft -= OnPlayerLeft;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
       _uiInputs = FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None);
    }

    public void GameStarted(string levelName)
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined; //Cannot join mid game
        foreach (PlayerInput playerInput in JoinedPlayers.Values)
        {
            playerInput.SwitchCurrentActionMap("Player");
            playerInput.ActivateInput();
        }
        
        SceneManager.LoadScene(levelName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Menu")
        {
            GameManager.Instance.SpawnPlayers();
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
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
        
        OnLobbyPlayerCountChanged?.Invoke(leavingIndex + 1);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            throw new Exception("No active Player Input Joining");
        }
        int intPlayerEnum = playerInput.playerIndex + 1;
        PlayerEnum playerEnum = (PlayerEnum) intPlayerEnum;

        switch (playerEnum)
        {
            case PlayerEnum.Player1:
                Player.PlayerColorDict[playerEnum] = Color.red;
                break;
            case PlayerEnum.Player2:
                Player.PlayerColorDict[playerEnum] = Color.green;
                break;
            case PlayerEnum.Player3:
                Player.PlayerColorDict[playerEnum] = Color.blue;
                break;
            case PlayerEnum.Player4:
                Player.PlayerColorDict[playerEnum] = Color.yellow;
                break;
            default:
                Debug.LogWarning("Maximum of " + GameConstants.NB_PLAYERS + " players reached. Extra device ignored.");
                return;
        }
        
        DontDestroyOnLoad(playerInput.gameObject);
        JoinedPlayers[playerEnum] = playerInput;

        if (JoinedPlayers.Count == 1)
        {
            GiveUIControl(playerInput);
        }
        else
        {
            playerInput.DeactivateInput();
        }
        
        OnLobbyPlayerCountChanged?.Invoke(intPlayerEnum);
    }

    private void GiveUIControl(PlayerInput playerInput)
    {
        playerInput.SwitchCurrentActionMap("UI");
        playerInput.ActivateInput();

        foreach (var uiInput in _uiInputs)
        {
            uiInput.actionsAsset = playerInput.actions;
        }
        
        EventSystem.current.SetSelectedGameObject(null);
        
        Selectable first = Selectable.allSelectablesArray.First();
        EventSystem.current.SetSelectedGameObject(first.gameObject);
    }
}