using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
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
    private MenuUIManager menuUIManager;

    public static LobbyManager Instance { get; private set; }

    public static readonly Dictionary<PlayerInput, float> JoinTimes = new();
    public static readonly Dictionary<PlayerEnum, PlayerInput> JoinedPlayers = new ();

    public static bool ItemsActivated = true;
    public static bool TutorialActivated = true;
    public static bool CVDActivated;
    public static int CVDIndex = 0;

    private PlayerInputManager _inputManager;

    private InputSystemUIInputModule[] _uiInputs;

    private void Start()
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
       if (SceneManager.GetActiveScene().name == "Menu")
       {
           foreach (var player in JoinedPlayers)
           {
               player.Value.DeactivateInput();
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
        foreach (PlayerInput playerInput in JoinedPlayers.Values)
        {
            playerInput.SwitchCurrentActionMap("Player");
            playerInput.ActivateInput();
        }
        
        SceneManager.LoadScene(levelIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Menu"  && scene.name != "EndGame")
        {
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
        
        menuUIManager.TogglePlayerUI(leavingIndex + 1);
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
                Player.PlayerColorDict[playerEnum] = new Color(255f/255f, 41f/255f, 117f/255f); 
                break;
            case PlayerEnum.Player2:
                Player.PlayerColorDict[playerEnum] = new Color(0f, 245f/255f, 212f/255f);
                break;
            case PlayerEnum.Player3:
                Player.PlayerColorDict[playerEnum] = new Color(107f/255f, 44f/255f, 255f/255f);
                break;
            case PlayerEnum.Player4:
                Player.PlayerColorDict[playerEnum] = new Color(255f/255f, 255f/255f, 33f/255f);
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

        menuUIManager.TogglePlayerUI(intPlayerEnum);
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

    private void GiveUIControl(PlayerInput playerInput, bool firstPlayer = true)
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
}