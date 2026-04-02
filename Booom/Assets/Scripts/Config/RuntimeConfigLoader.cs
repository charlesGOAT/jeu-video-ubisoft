using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ItemSpawnerData
{
    public int MaxItems = 0;
    public float TimeBetweenSpawns = 0;
}

[Serializable]
public class RuntimeConfigData
{
    public bool IsSpreadingMode = true;
    public bool IsRandomMap = true;
    public bool IsBonusSpeed = true;
    public bool HighlightOwnColor = true;
    public SpawnMode SpawnMode = SpawnMode.Fixed;
    public bool IsDropFromSky = true;
    public float GameDuration = 120f;
    public float MovementSpeed = 15f;
    public int FrozenTileDuration = 30;
    public float ColorBoost = 1.5f;
    public float ColorDebuff = 0.85f;
    public float NormalBombTimer = 3.0f;
    public float FastBombTimer = 1.0f;
    public float PopUpDuration = 2.0f;
    public bool ShouldBombCollideWithPlayers = true;
    public float AirStateDuration = 1.0f;
    public float HitTimeDuration = 1.5f;

    public ItemSpawnerData PaintBrushItemSpawnerData = new()
    {
        MaxItems = 2,
        TimeBetweenSpawns = 10
    };
    public ItemSpawnerData ChainedBombItemSpawnerData = new()
    {
        MaxItems = 2,
        TimeBetweenSpawns = 10
    };
    public ItemSpawnerData TargetBombItemSpawnerData = new()
    {
        MaxItems = 2,
        TimeBetweenSpawns = 10
    };
    public ItemSpawnerData FreezeBombItemSpawnerData = new()
    {
        MaxItems = 2,
        TimeBetweenSpawns = 10
    };
    public Dictionary<int, float> SpeedBoostPerKill = new ()
    {
        {0, 1f},
        {1, 1.25f},
        {2, 1.5f},
        {3, 1.75f},
        {4, 2f},
        {5, 2.25f}
    };
    public Dictionary<int, int> RangeBoostPerKill = new ()
    {
        {0, 0},
        {2, 1},
        {4, 2},
        {6, 3}
    };

    public List<KeyValuePair> BombEvents = new();
    public List<KeyValuePairText> TextEvents = new ()
    {
        new (2,0, "Start placing bombs to spread your zone!")
    };
}

public static class RuntimeConfigLoader
{
    private const string CONFIG_FILE_NAME = "gameConfig.json";

    private static bool _isLoaded;
    private static RuntimeConfigData _cachedConfig;

    public static RuntimeConfigData GetConfig()
    {
        if (_isLoaded)
        {
            return _cachedConfig;
        }

        string configPath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);

        if (!File.Exists(configPath))
        {
            Debug.LogWarning($"Config file not found at '{configPath}'. Using default values.");
            _cachedConfig = new RuntimeConfigData();
            _isLoaded = true;
            return _cachedConfig;
        }

        try
        {
            string jsonContent = File.ReadAllText(configPath);
            _cachedConfig = JsonUtility.FromJson<RuntimeConfigData>(jsonContent);

            if (_cachedConfig == null)
            {
                Debug.LogWarning("Config file is empty or invalid. Using default values.");
                _cachedConfig = new RuntimeConfigData();
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to read config file: {exception.Message}. Using default values.");
            _cachedConfig = new RuntimeConfigData();
        }

        _isLoaded = true;
        return _cachedConfig;
    }
}
