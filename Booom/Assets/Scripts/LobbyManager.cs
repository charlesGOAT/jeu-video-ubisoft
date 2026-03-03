using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public delegate void LobbyPlayerCountChanged(int playerCount);

public class LobbyManager : MonoBehaviour
{
    public event LobbyPlayerCountChanged OnLobbyPlayerCountChanged;
    
    public static readonly List<PlayerInput> JoinedPlayers = new ();
    private PlayerInputManager _inputManager;

    private void Awake()
    {
        _inputManager = GetComponent<PlayerInputManager>();
        DontDestroyOnLoad(gameObject);
        
        if (FindObjectsByType<LobbyManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
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
        JoinedPlayers.Add(playerInput);

        if (playerEnum == PlayerEnum.Player1)
        {
            playerInput.SwitchCurrentActionMap("UI"); //Only Player 1 can navigate in the menu
        }
        else
        {
            playerInput.DeactivateInput(); //Idk if its problematic to not deactivate it
        }
        
        OnLobbyPlayerCountChanged?.Invoke(intPlayerEnum);
    }
}