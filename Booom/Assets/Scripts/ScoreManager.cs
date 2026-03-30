using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void ScoreChangedEventHandler(PlayerEnum player, int score);

public class ScoreManager : MonoBehaviour
{
    public readonly Dictionary<PlayerEnum, HashSet<Vector2Int>> AcquiredTilesByPlayer = new();
    
    public event ScoreChangedEventHandler OnScoreChanged;

    private void Start()
    {
        foreach(PlayerEnum player in Enum.GetValues(typeof(PlayerEnum)))
        {
            AcquiredTilesByPlayer[player] = new HashSet<Vector2Int>();
        }
    }

    public void NewElimination(in PlayerEnum player)
    {
        if (player == PlayerEnum.None) return;
        Player.ActivePlayers[player].NbKills++;
    }
    
    public void AcquireNewTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
        
        AcquiredTilesByPlayer[player].Add(tile);

        int newScore = CalculateScore(player);
        OnScoreChanged?.Invoke(player, newScore);
        
        if (newScore >= GameManager.Instance.GridManager.CapturableTilesCount)
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void LoseTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
        
        AcquiredTilesByPlayer[player].Remove(tile);
        OnScoreChanged?.Invoke(player, CalculateScore(player));
    }

    private int CalculateScore(in PlayerEnum player) => (int)(((float)AcquiredTilesByPlayer[player].Count / GameManager.Instance.GridManager.CapturableTilesCount) * 100);
    
    public PlayerEnum FindPlayerWithMostGround()
    {
        PlayerEnum playerMax = PlayerEnum.None;
        int currentMax = 0;
        List<PlayerEnum> equalMax = new();

        foreach(PlayerEnum playerNb in Player.ActivePlayers.Keys)
        {
            if (playerNb == PlayerEnum.None) continue;
            if (AcquiredTilesByPlayer[playerNb].Count > currentMax)
            {
                playerMax = playerNb;
                currentMax = AcquiredTilesByPlayer[playerNb].Count;
                equalMax.Clear();
                equalMax.Add(playerMax);
            }
            else if (AcquiredTilesByPlayer[playerNb].Count == currentMax && currentMax != 0)
            {
                equalMax.Add(playerNb);
            }
        }

        if (equalMax.Count > 1)
        {
            var random = new System.Random();
            int ind = random.Next(0, equalMax.Count);
            playerMax = equalMax[ind];
        }

        return playerMax;
    }
}
