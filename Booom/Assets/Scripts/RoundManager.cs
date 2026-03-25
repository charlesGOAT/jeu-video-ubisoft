using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RoundManager
{
    private static readonly List<int> mapsToPlay = new () {3};
    private static readonly List<int> _mapsPlayed = new();
    private static readonly List<PlayerEnum> _gameWonPlayer = new();

    private static RuntimeConfigData _runtimeConfig;
    private static bool _isRuntimeConfigSet = false;
    private static int _lastMapIndex = 0;
    
    private static Dictionary<PlayerEnum, int> _playerWinsDict = new()
    {
        { PlayerEnum.Player1, 0 },
        { PlayerEnum.Player2, 0 },
        { PlayerEnum.Player3, 0 },
        { PlayerEnum.Player4, 0 },
    };

    private static bool IsRandomMap
    {
        get
        {
#if !UNITY_EDITOR
            if (!_isRuntimeConfigSet)
            {
                _runtimeConfig = RuntimeConfigLoader.GetConfig();
                _isRuntimeConfigSet = true;
            }

            return _runtimeConfig.IsRandomMap;
#endif
            return true;
        }
    }
    
    public static int FindNextMap()
    {
        int newMapIndex = -1;

        if (IsRandomMap)
        {
            System.Random rand = new();
            int count = 0;
            do
            {
                newMapIndex = rand.Next(0, mapsToPlay.Count);

                if (count++ >= mapsToPlay.Count)
                {
                    Debug.LogError("There's not enough maps, playing already played random map");
                    break;
                }
            } 
            while (_mapsPlayed.Contains(mapsToPlay[newMapIndex]));
        }
        else
        {
            newMapIndex = ++_lastMapIndex;
        }

        int nextMap = mapsToPlay[newMapIndex];
        _mapsPlayed.Add(nextMap);
        return nextMap;
    }
    
    public static void CleanGame()
    {
        _mapsPlayed.Clear();
        _playerWinsDict.Clear();
        foreach (PlayerEnum player in Enum.GetValues(typeof(PlayerEnum)))
        {
            _playerWinsDict[player] = 0;
        }
        _gameWonPlayer.Clear();
        _lastMapIndex = 0;
    }

    public static void LoadEndGameData()
    {
        var playerRanks = _playerWinsDict
            .OrderByDescending(x => x.Value)
            .Select((x, index) => (Player: x.Key, Rank: index))
            .ToDictionary(item => item.Player, item => item.Rank);
        
        foreach (Player player in Player.ActivePlayers)
        {
            if (player.PlayerNb == PlayerEnum.None) continue;
            EndGameUIManager.PlayerRank[player.PlayerNb] = playerRanks[player.PlayerNb];
        }

        EndGameUIManager.ShouldEndGame = true;
        EndGameUIManager.PlayerWonGame = new(_gameWonPlayer);
    }

    public static void LoadEndRoundData()
    {
        foreach (Player player in Player.ActivePlayers)
        {
            if (player.PlayerNb == PlayerEnum.None) continue;
            EndGameUIManager.PlayerRank[player.PlayerNb] = (int)player.PlayerNb - 1;
        }

        EndGameUIManager.NextSceneIndex = FindNextMap();
        EndGameUIManager.PlayerWonGame = new(_gameWonPlayer);
    }

    public static bool ShouldEndGame(in PlayerEnum winner)
    {
        _playerWinsDict[winner]++;
        _gameWonPlayer.Add(winner);
        return _playerWinsDict[winner] == 2;
    }
}
