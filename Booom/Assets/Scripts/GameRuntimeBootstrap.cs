using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class GameRuntimeBootstrap
{
    private const string MainMenuSceneName = "MainMenu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureGameplaySystems(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureGameplaySystems(scene);
    }

    private static void EnsureGameplaySystems(Scene scene)
    {
        if (!scene.isLoaded)
            return;

        // Never show gameplay HUD in the main menu.
        if (string.Equals(scene.name, MainMenuSceneName, StringComparison.OrdinalIgnoreCase))
        {
            var hud = GameObject.Find("GameStatsHUD");
            if (hud != null)
                UnityEngine.Object.Destroy(hud);

            var manager = GameObject.Find("GameStatsManager");
            if (manager != null)
                UnityEngine.Object.Destroy(manager);

            return;
        }

        if (UnityEngine.Object.FindFirstObjectByType<GameStatsManager>() == null)
        {
            var go = new GameObject("GameStatsManager");
            go.AddComponent<GameStatsManager>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.Log("GameRuntimeBootstrap: spawned GameStatsManager");
        }

        if (UnityEngine.Object.FindFirstObjectByType<GameStatsUI>() == null)
        {
            var go = new GameObject("GameStatsHUD");
            go.AddComponent<UIDocument>();
            go.AddComponent<GameStatsUI>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.Log("GameRuntimeBootstrap: spawned GameStatsHUD");
        }
    }
}
