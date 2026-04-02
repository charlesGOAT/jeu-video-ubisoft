using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MapTestLobbyManager : MonoBehaviour
{
    private PlayerInputManager _inputManager;

    private void Start()
    {
        _inputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        _inputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        _inputManager.onPlayerJoined -= OnPlayerJoined;
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
                throw new Exception("Player Input Manager tried to create invalid Player");
        }
    }
}