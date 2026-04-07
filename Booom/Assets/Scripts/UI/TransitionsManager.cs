using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


public class TransitionsManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer fadeOut;

    [SerializeField] 
    private RawImage image;
    
    private static readonly int TransitionColor = Shader.PropertyToID("_BackgroundColor");

    [SerializeField] 
    private Canvas canvas;
    
    private static PlayerEnum _lastWinner;

    private bool _isInGameScene;
    
    public static TransitionsManager Instance { get; private set; }

    private bool _shouldContinueTransition = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        canvas.worldCamera = Camera.main;
        SceneManager.activeSceneChanged += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
        fadeOut.Prepare();
        image.texture = fadeOut.texture;
    }

    private void OnSceneLoaded(Scene scene, Scene scene2)
    {
        canvas.worldCamera = Camera.main;

        if (_shouldContinueTransition)
        {
            _shouldContinueTransition = false;
            fadeOut.Play();
        }
    }

    public void LoadSceneFadeOut(int sceneIndex, in PlayerEnum winner, in string sceneName = "")
    {
        _lastWinner = winner;
        StartCoroutine(ManageFadeOut(sceneIndex, sceneName));
    }

    private IEnumerator ManageFadeOut(int sceneIndex, string sceneName = "")
    {
        SoundManager.Instance.OnSceneTransition();
        fadeOut.Play();
        image.texture = fadeOut.texture;
        image.material.SetColor(TransitionColor, Player.PlayerColorDict[_lastWinner]);
        _shouldContinueTransition = true;
        yield return new WaitUntil(() => fadeOut.time >= 1.4f);
        fadeOut.Pause();
        if (sceneName.Equals(String.Empty)) SceneManager.LoadScene(sceneIndex);
        else SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneLoaded;
    }
}
