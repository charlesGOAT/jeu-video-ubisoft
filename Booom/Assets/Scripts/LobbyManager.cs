using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public static List<PlayerInput> joinedPlayers = new List<PlayerInput>();

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput != null)
        {
            switch (playerInput.playerIndex)
            {
                case 0:
                    Player.PlayerColorDict[PlayerEnum.Player1] = Color.red;
                    break;
                case 1:
                    Player.PlayerColorDict[PlayerEnum.Player2] = Color.green;
                    break;
                case 2:
                    Player.PlayerColorDict[PlayerEnum.Player3] = Color.blue;
                    break;
                case 3:
                    Player.PlayerColorDict[PlayerEnum.Player4] = Color.yellow;
                    break;
                default:
                    throw new Exception("Player Input Manager tried to create invalid Player");
            }
        }
        else
        {
            throw new Exception("There's no active player input");
        }
        
        PlayerEnum playerEnum = (PlayerEnum) playerInput.playerIndex + 1;
        Color c = Player.PlayerColorDict[playerEnum];

        // GameManager.Instance.UIManager.PlayerAdded(playerEnum, c);
        
        Debug.Log("Player joined : "  + playerEnum);
    }
}