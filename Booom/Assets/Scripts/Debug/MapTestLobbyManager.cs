using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MapTestLobbyManager : MonoBehaviour
{
    private PlayerInputManager _inputManager;

    private void Awake()
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

        if (playerEnum is < PlayerEnum.Player1 or > PlayerEnum.Player4)
        {
            throw new Exception("Player Input Manager tried to create invalid Player");
        }

        Player.PlayerColorDict[playerEnum] = GameConstants.GetPlayerColor(playerEnum);
    }
}
