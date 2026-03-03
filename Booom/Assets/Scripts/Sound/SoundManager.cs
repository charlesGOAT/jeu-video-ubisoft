using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
struct Audio
{
    public AudioClip audioClip;
    public float volume;
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [Header("--- Menu sounds ---")]
    [SerializeField] 
    private Audio buttonClickedAudio;
    [SerializeField] 
    private Audio backgroundMenuMusic;
    [Space(3)]
    
    [Header("--- Bomb sounds ---")]
    [SerializeField] 
    private Audio bombFusedAudio;
    
    [SerializeField] 
    private Audio bombExplodedAudio;
    
    [Space(3)]

    [Header("--- Game music ---")]
    [SerializeField] 
    private Audio gameMusic;
    [Space(3)]

    private AudioSource _audioSourceSFX;
    private AudioSource _audioSourceMusic;

    private bool _isInMenu = false;

    private bool IsSceneMenu(in Scene scene) => scene.name.Contains("menu", StringComparison.OrdinalIgnoreCase);

    public static SoundManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeAudioSources();
        StartBackgroundMusic();
        
        VerifyAudioClipsPresent();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void InitializeAudioSources()
    {
        var audioSources = gameObject.GetComponents<AudioSource>();
        if (audioSources.Length < 2) throw new Exception("There should be at least two audio sources");
        _audioSourceSFX = audioSources[0];
        _audioSourceMusic = audioSources[1];
        _audioSourceMusic.loop = true;
    }

    private void StartBackgroundMusic()
    {
        _isInMenu = IsSceneMenu(SceneManager.GetActiveScene());
        if (_isInMenu)
            PlayAudioSourceMusic(backgroundMenuMusic);
        else
            PlayAudioSourceMusic(gameMusic);
    }

    private void PlayAudioSourceMusic(in Audio clip)
    {
        _audioSourceMusic.clip = clip.audioClip;
        _audioSourceMusic.volume = clip.volume;
        _audioSourceMusic.Play();
    }

    private void VerifyAudioClipsPresent()
    {
        if (bombExplodedAudio.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(bombExplodedAudio.audioClip)} cannot be null");
        }
        if (bombFusedAudio.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(bombFusedAudio.audioClip)} cannot be null");
        }
        if (buttonClickedAudio.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(buttonClickedAudio.audioClip)} cannot be null");
        }
        if (backgroundMenuMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(backgroundMenuMusic.audioClip)} cannot be null");
        }
        if (gameMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(gameMusic.audioClip)} cannot be null");
        }
    }
    
    public void OnBombFused(Bomb bomb)
    {
        AudioSource.PlayClipAtPoint(bombFusedAudio.audioClip, bomb.transform.position, bombFusedAudio.volume);
    }

    public void OnBombExploded(Bomb bomb)
    {
        AudioSource.PlayClipAtPoint(bombExplodedAudio.audioClip, bomb.transform.position, bombExplodedAudio.volume);
    }

    public void OnMenuButtonPressed()
    {
        _audioSourceSFX.PlayOneShot(buttonClickedAudio.audioClip, buttonClickedAudio.volume);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsSceneMenu(scene))
        {
            // todo : voir s'il va y avoir plus qu'une menu scene / plus qu'une toune pour ces menus
            
            if (_isInMenu) return;  // assumant qu'il n'y a qu'une toune pour tous les menus

            _isInMenu = true;
            PlayAudioSourceMusic(backgroundMenuMusic);
        }
        else
        {
            _isInMenu = false;
            PlayAudioSourceMusic(gameMusic);  // todo : voir s'il y a plus qu'une toune de jeu
        }
    }
}
