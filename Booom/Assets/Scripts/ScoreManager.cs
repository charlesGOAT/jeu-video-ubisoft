using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public delegate void ScoreChangedEventHandler(PlayerEnum player, int score);

public class EndGameResult
{
    public bool IsDraw { get; }
    public PlayerEnum Winner { get; }
    public IReadOnlyList<PlayerEnum> TiedPlayers { get; }

    public EndGameResult(PlayerEnum winner)
    {
        IsDraw = false;
        Winner = winner;
        TiedPlayers = new List<PlayerEnum>();
    }

    public EndGameResult(IReadOnlyList<PlayerEnum> tiedPlayers)
    {
        IsDraw = true;
        Winner = PlayerEnum.None;
        TiedPlayers = tiedPlayers;
    }
}

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
        
        _acquiredTilesByPlayer[(int)player - 1].Add(tile);

        int newScore = _acquiredTilesByPlayer[(int)player - 1].Count;
        OnScoreChanged?.Invoke(player, newScore);
        
        if (newScore >= GameManager.Instance.GridManager.capturableTilesCount)
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void LoseTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
            
        _acquiredTilesByPlayer[(int)player - 1].Remove(tile);
        
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

    public EndGameResult GetEndGameResultByTilesThenKills()
    {
        List<int> tileLeaders = new();
        int maxTiles = -1;

        for (int i = 0; i < _acquiredTilesByPlayer.Length; i++)
        {
            int tileCount = _acquiredTilesByPlayer[i].Count;
            if (tileCount > maxTiles)
            {
                maxTiles = tileCount;
                tileLeaders.Clear();
                tileLeaders.Add(i);
            }
            else if (tileCount == maxTiles)
            {
                tileLeaders.Add(i);
            }
        }

        if (tileLeaders.Count == 1)
        {
            return new EndGameResult((PlayerEnum)(tileLeaders[0] + 1));
        }

        List<int> killLeaders = new();
        int maxKills = -1;

        foreach (int playerIndex in tileLeaders)
        {
            PlayerEnum player = (PlayerEnum)(playerIndex + 1);
            int kills = GetPlayerKills(player);

            if (kills > maxKills)
            {
                maxKills = kills;
                killLeaders.Clear();
                killLeaders.Add(playerIndex);
            }
            else if (kills == maxKills)
            {
                killLeaders.Add(playerIndex);
            }
        }

        if (killLeaders.Count == 1)
        {
            return new EndGameResult((PlayerEnum)(killLeaders[0] + 1));
        }

        List<PlayerEnum> tiedPlayers = killLeaders
            .Select(index => (PlayerEnum)(index + 1))
            .ToList();

        return new EndGameResult(tiedPlayers);
    }

    private int GetPlayerKills(PlayerEnum player)
    {
        Player playerComponent = Player.ActivePlayers.FirstOrDefault(activePlayer => activePlayer.PlayerNb == player);
        return playerComponent == null ? 0 : playerComponent.NbKills;
    }

    public HashSet<Vector2Int>[] GetAcquiredTilesByPlayer()
    {
        return _acquiredTilesByPlayer;
    }
}
