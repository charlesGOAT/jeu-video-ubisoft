using System;
using UnityEngine;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    [SerializeField]
    private bool persistAcrossScenes = false;

    public int[] BombsPlaced { get; private set; }
    public int[] TilesPainted { get; private set; }
    public int[] TilesStolen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
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
        if (player == PlayerEnum.None)
            return;

        int idx = ((int)player) - 1;
        if (idx < 0 || idx >= BombsPlaced.Length)
            return;

        BombsPlaced[idx]++;
    }

    public void OnTilePainted(PlayerEnum player)
    {
        if (player == PlayerEnum.None)
            return;

        int idx = ((int)player) - 1;
        if (idx < 0 || idx >= TilesPainted.Length)
            return;

        TilesPainted[idx]++;
    }

    public void OnTileStolen(PlayerEnum player)
    {
        if (player == PlayerEnum.None)
            return;

        int idx = ((int)player) - 1;
        if (idx < 0 || idx >= TilesStolen.Length)
            return;

        TilesStolen[idx]++;
    }
}
