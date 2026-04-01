using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutoManager : MonoBehaviour
{
    private readonly Dictionary<PlayerEnum, bool> _playersBombed = new();
    private readonly Dictionary<PlayerEnum, bool> _playersFinished = new();
    
    private TutoUIManager _tutoUIManager;
    
    void Start()
    {
        _tutoUIManager = FindAnyObjectByType<TutoUIManager>();
        
        foreach (var player in LobbyManager.JoinedPlayers)
        {
            _playersBombed[player.Key] = false;
            _playersFinished[player.Key] = false;

            Player.ActivePlayers[player.Key].CanMove = false;
        }
        
        GameManager.Instance.ScoreManager.OnScoreChanged += UpdateTuto;
    }

    private void UpdateTuto(PlayerEnum player, int score)
    {
        if (player == PlayerEnum.None || score < 3) return;

        Player.ActivePlayers[player].CanMove = true;
        
        if (score < 9)
        {
            if (!_playersBombed[player])
            {
                _playersBombed[player] = true;
                _tutoUIManager.UpdatePlayerText(player);
            }
        }
        else
        {
            _playersFinished[player] = true;
            _tutoUIManager.PlayerEndTuto(player);
            if (_playersFinished.All(p => p.Value))
                _tutoUIManager.EndTuto();
        }
    }
}
