using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public delegate void LobbyPlayerCountChanged(int playerCount);

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    public event LobbyPlayerCountChanged OnLobbyPlayerCountChanged;
    
    public static readonly List<PlayerInput> JoinedPlayers = new ();
    private PlayerInputManager _inputManager;

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GameStarted()
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined; //Cannot join mid game
        foreach (PlayerInput playerInput in JoinedPlayers)
        {
            playerInput.SwitchCurrentActionMap("Player");
            playerInput.ActivateInput();
        }
        
        SceneManager.LoadScene("TheRing");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Menu")
        {
            GameManager.Instance.SpawnPlayers();
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null)
        {
            throw new Exception("No active Player Input");
        }
        PlayerEnum playerEnum = (PlayerEnum) playerInput.playerIndex + 1;

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
                throw new Exception("Player Input Manager tried to create invalid Player");
        }
        
        DontDestroyOnLoad(playerInput.gameObject);
        JoinedPlayers.Add(playerInput);

        if (playerEnum == PlayerEnum.Player1)
        {
            playerInput.SwitchCurrentActionMap("UI"); //Only Player 1 can navigate in the menu
        }
        else
        {
            playerInput.DeactivateInput(); //Idk if its problematic to not deactivate it
        }
        
        OnLobbyPlayerCountChanged?.Invoke((int)playerEnum);
    }
}