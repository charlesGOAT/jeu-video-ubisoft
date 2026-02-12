using System;
using UnityEngine;

public class GameStatsManager : MonoBehaviour
{
    private static GameStatsManager _instance;

    public static GameStatsManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFirstObjectByType<GameStatsManager>();
            if (_instance != null) return _instance;
            var go = new GameObject("GameStatsManager");
            _instance = go.AddComponent<GameStatsManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    public int[] BombsPlaced { get; private set; }
    public int[] TilesPainted { get; private set; }
    public int[] TilesStolen { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        BombsPlaced = new int[GameConstants.NB_PLAYERS];
        TilesPainted = new int[GameConstants.NB_PLAYERS];
        TilesStolen = new int[GameConstants.NB_PLAYERS];
    }

    public void ResetAll()
    {
        Array.Clear(BombsPlaced, 0, BombsPlaced.Length);
        Array.Clear(TilesPainted, 0, TilesPainted.Length);
        Array.Clear(TilesStolen, 0, TilesStolen.Length);
    }

    public void OnBombPlaced(PlayerEnum player)
    {
        int idx = ((int)player) - 1;
        if (idx < 0 || idx >= BombsPlaced.Length) return;
        BombsPlaced[idx]++;
    }

    public void OnTilePainted(PlayerEnum player)
    {
        int idx = ((int)player) - 1;
        if (idx < 0 || idx >= TilesPainted.Length) return;
        TilesPainted[idx]++;
    }

    public void OnTileStolen(PlayerEnum player)
    {
        int idx = ((int)player) - 1;
        if (idx < 0 || idx >= TilesStolen.Length) return;
        TilesStolen[idx]++;
    }
}
