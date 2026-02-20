using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    private readonly HashSet<Vector2Int>[] _acquiredTilesByPlayer = new HashSet<Vector2Int>[GameConstants.NB_PLAYERS];
    private readonly Dictionary<PlayerEnum, int> _eliminationsPerPlayer = new(GameConstants.NB_PLAYERS); //We could add other stats like deaths or items used

    private void Start()
    {
        for (int i = 0; i < _acquiredTilesByPlayer.Length; i++)
        {
            _acquiredTilesByPlayer[i] = new HashSet<Vector2Int>();
        }
        for (int i = 1; i <= GameConstants.NB_PLAYERS; i++)
        {
            _eliminationsPerPlayer.Add((PlayerEnum)i, 0);
        }
    }

    public void NewElimination(PlayerEnum player)
    {
        if (player != PlayerEnum.None)
            _eliminationsPerPlayer[player]++;

        if (_eliminationsPerPlayer[player] >= GameConstants.ELIMS_TO_WIN)
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void AcquireNewTile(PlayerEnum player, Vector2Int tile)
    {
        if (player == PlayerEnum.None) return;
        
        _acquiredTilesByPlayer[(int)player - 1].Add(tile);

        if (_acquiredTilesByPlayer[(int)player - 1].Count == GameManager.Instance.GridManager.capturableTilesCount) 
        {
            GameManager.Instance.EndGame();
        }
    }
    
    public void LoseTile(PlayerEnum player, Vector2Int tile)
    {
        if (player != PlayerEnum.None)
            _acquiredTilesByPlayer[(int)player - 1].Remove(tile);
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
