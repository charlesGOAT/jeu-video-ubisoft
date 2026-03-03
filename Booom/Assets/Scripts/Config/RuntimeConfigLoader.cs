using System;
using System.IO;
using UnityEngine;

[Serializable]
public class RuntimeConfigData
{
    public bool IsSpreadingMode = true;
    public bool IsBonusSpeed = true;
    public SpawnMode SpawnMode = SpawnMode.Fixed;
    public bool IsDropFromSky = false;
    public float GameDuration = 120f;
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
