using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RoundManager
{
    public static readonly List<int> MapsToPlay = new () {1,2,3,4,5};
    private static readonly List<int> _mapsPlayed = new();
    private static readonly List<PlayerEnum> _gameWonPlayer = new();

    private static RuntimeConfigData _runtimeConfig;
    private static bool _isRuntimeConfigSet = false;
    public static int LastMapIndex = 0;
    
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
                newMapIndex = rand.Next(0, MapsToPlay.Count);

                if (count++ >= MapsToPlay.Count)
                {
                    Debug.LogError("There's not enough maps, playing already played random map");
                    break;
                }
            } 
            while (_mapsPlayed.Contains(MapsToPlay[newMapIndex]));
        }
        else
        {
            newMapIndex = LastMapIndex++;
        }

        int nextMap = MapsToPlay[newMapIndex];
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
        LastMapIndex = 0;
    }

    public static void LoadEndGameData()
    {
        var playerRanks = _playerWinsDict
            .Where(x => Player.ActivePlayers.Keys.Contains(x.Key))
            .OrderByDescending(x => x.Value)
            .Select((x, index) => (Player: x.Key, Rank: index))
            .ToDictionary(item => item.Player, item => item.Rank);
        
        foreach (Player player in Player.ActivePlayers.Values)
        {
            if (player.PlayerNb == PlayerEnum.None) continue;
            EndGameUIManager.PlayerRank[player.PlayerNb] = playerRanks[player.PlayerNb];
        }

        EndGameUIManager.ShouldEndGame = true;
        EndGameUIManager.PlayerWonGame = new(_gameWonPlayer);
    }

    public static void LoadEndRoundData()
    {
        int i = 0;
        foreach (Player player in Player.ActivePlayers.Values)
        {
            if (player.PlayerNb == PlayerEnum.None) continue;
            EndGameUIManager.PlayerRank[player.PlayerNb] = i++;
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
