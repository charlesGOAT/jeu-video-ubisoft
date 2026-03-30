using System.Collections.Generic;
using UnityEngine;

public delegate void ScoreChangedEventHandler(PlayerEnum player, int score);

public class ScoreManager : MonoBehaviour
{
    private readonly HashSet<Vector2Int>[] _acquiredTilesByPlayer = new HashSet<Vector2Int>[GameConstants.NB_PLAYERS];
    
    public event ScoreChangedEventHandler OnScoreChanged;

    private void Start()
    {
        for (int i = 0; i < _acquiredTilesByPlayer.Length; i++)
        {
            _acquiredTilesByPlayer[i] = new HashSet<Vector2Int>();
        }
    }

    public void NewElimination(PlayerEnum player)
    {
        if (player == PlayerEnum.None) return;
        
        Player.ActivePlayers[(int)player - 1].NbKills++;
    }
    
    public void AcquireNewTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
        
        int playerNb = (int)player - 1;
        _acquiredTilesByPlayer[playerNb].Add(tile);

        int newScore = CalculateScore(playerNb);
        OnScoreChanged?.Invoke(player, newScore);
        
        if (newScore >= GameManager.Instance.GridManager.CapturableTilesCount)
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void LoseTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
        
        int playerNb = (int)player - 1;
        _acquiredTilesByPlayer[playerNb].Remove(tile);
        
        OnScoreChanged?.Invoke(player, CalculateScore(playerNb));
    }

    private int CalculateScore(int player) => (int)(((float)_acquiredTilesByPlayer[player].Count / GameManager.Instance.GridManager.CapturableTilesCount) * 100);
    
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
