using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public delegate void ScoreChangedEventHandler(PlayerEnum player, int score);

public class ScoreManager : MonoBehaviour
{
    private readonly HashSet<Vector2Int>[] _acquiredTilesByPlayer = new HashSet<Vector2Int>[GameConstants.NB_PLAYERS];
    private bool _spreadMode = false;
    
    public event ScoreChangedEventHandler OnScoreChanged;

    private void Start()
    {
        for (int i = 0; i < _acquiredTilesByPlayer.Length; i++)
        {
            _acquiredTilesByPlayer[i] = new HashSet<Vector2Int>();
        }
        
        _spreadMode = GameManager.Instance.IsSpreadingMode;
    }

    public void NewElimination(PlayerEnum player)
    {
        if (player == PlayerEnum.None) return;
        
        Player.ActivePlayers[(int)player - 1].NbKills++;

        if (_spreadMode) return;
        
        OnScoreChanged?.Invoke(player, Player.ActivePlayers[(int)player - 1].NbKills);
        if (Player.ActivePlayers[(int)player - 1].NbKills >= GameConstants.ELIMS_TO_WIN)
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void AcquireNewTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
        
        _acquiredTilesByPlayer[(int)player - 1].Add(tile);

        if (!_spreadMode) return;

        int newScore = _acquiredTilesByPlayer[(int)player - 1].Count;
        OnScoreChanged?.Invoke(player, newScore);
        
        if (newScore >= 3) 
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void LoseTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
            
        _acquiredTilesByPlayer[(int)player - 1].Remove(tile);
        
        if (!_spreadMode) return;
        
        OnScoreChanged?.Invoke(player, _acquiredTilesByPlayer[(int)player - 1].Count);
    }
    
    public PlayerEnum FindPlayerWithMostGround()
    {
        int indexMax = -1;
        int currentMax = 0;
        List<int> equalMax = new();

        for (int i = 0; i < _acquiredTilesByPlayer.Length; ++i)
        {
            if (_acquiredTilesByPlayer[i].Count > currentMax)
            {
                indexMax = i;
                currentMax = _acquiredTilesByPlayer[i].Count;
                equalMax.Clear();
                equalMax.Add(indexMax);
            }
            else if (_acquiredTilesByPlayer[i].Count == currentMax && currentMax != 0)
            {
                equalMax.Add(i);
            }
        }

        if (equalMax.Count > 1)
        {
            var random = new System.Random();
            int ind = random.Next(0, equalMax.Count);
            indexMax = equalMax[ind];
        }

        return (PlayerEnum)(indexMax + 1);
    }

    public HashSet<Vector2Int>[] GetAcquiredTilesByPlayer()
    {
        return _acquiredTilesByPlayer;
    }
}
