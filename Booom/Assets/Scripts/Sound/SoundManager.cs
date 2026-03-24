using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
struct Audio
{
    public AudioClip audioClip;
    [Range(0,1)]
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
    private AudioSource bombFusedAudio;
    [SerializeField] 
    private AudioSource bombExplodedAudio;
    [SerializeField] 
    private AudioSource genericBombEventSound;
    [SerializeField]
    private AudioSource targetBombMovingSound;
    [SerializeField]
    private Audio defenseBombSound;
    [Space(3)]
    
    [Header("--- Item sounds ---")]
    [SerializeField] 
    private Audio pickUpItemSound;
    [SerializeField]
    private AudioSource usePaintBrushSound;
    [Space(3)]
    
    [Header("--- Player sounds ---")]
    [SerializeField] 
    private AudioSource trampolineSound;
    [SerializeField]
    private AudioSource spikeSound;
    [SerializeField] 
    private AudioSource portalSound;
    [SerializeField] 
    private AudioSource bombHitPlayerSound;
    [SerializeField] 
    private AudioSource newKillStreakSound;
    [Space(3)]

    [Header("--- Game music ---")]
    [SerializeField] 
    private Audio gameStartsSound;
    [SerializeField] 
    private Audio mainTheme;
    [SerializeField] 
    private Audio acceleratedGameMusic;
    [SerializeField] 
    private Audio colorAlternationMusic;
    [SerializeField]
    private Audio victoryThemeMusic;
    [Space(3)]

    private AudioSource _audioSourceSFX;
    private AudioSource _audioSourceMusic;

    private static bool? _isInMenu = null;

    private bool IsSceneMenu(in Scene scene) => scene.name.Contains("menu", StringComparison.OrdinalIgnoreCase);
    private bool IsSceneEndGame(in Scene scene) => scene.name.Contains("EndGame", StringComparison.OrdinalIgnoreCase);

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
        VerifyAudioClipsPresent();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (!_isInMenu.HasValue)
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }

    private void InitializeAudioSources()
    {
        var audioSources = gameObject.GetComponents<AudioSource>();
        if (audioSources.Length < 2) throw new Exception("There should be at least two audio sources");
        _audioSourceSFX = audioSources[0];
        _audioSourceMusic = audioSources[1];
        _audioSourceMusic.loop = true;
    }

    private void PlayAudioSourceMusic(in Audio clip)
    {
        _audioSourceMusic.clip = clip.audioClip;
        _audioSourceMusic.volume = clip.volume;
        _audioSourceMusic.Play();
    }

    public void OnBombFused()
    {
        bombFusedAudio.Play();
    }

    public void OnBombExploded()
    {
        bombExplodedAudio.Play();
    }

    public void OnMenuButtonPressed()
    {
        _audioSourceSFX.PlayOneShot(buttonClickedAudio.audioClip, buttonClickedAudio.volume);
    }

    public void OnBombEvent()
    {
        genericBombEventSound.Play();
    }

    public void OnPickupItem()
    {
        _audioSourceSFX.PlayOneShot(pickUpItemSound.audioClip, pickUpItemSound.volume);
    }

    public void OnEnterTrampoline()
    {
        trampolineSound.Play();
    }
    
    public void OnEnterPortal()
    {
        portalSound.Play();
    }

    public void OnEnterSpikes()
    {
        spikeSound.Play();
    }

    public void OnPlayerHitByBomb()
    {
        bombHitPlayerSound.Play();
    }

    public void OnTargetBombMoving(bool isMoving)
    {
        if(isMoving) targetBombMovingSound.Play();
        else targetBombMovingSound.Stop();
    }

    public void OnDefenseBombExploded()
    {
        _audioSourceSFX.PlayOneShot(defenseBombSound.audioClip, defenseBombSound.volume);
    }

    public void OnPlayAcceleratedGameMusic()
    {
        PlayAudioSourceMusic(acceleratedGameMusic);
    }

    public void OnUsePaintBrush(bool isUsing)
    {
        if(isUsing) usePaintBrushSound.Play();
        else usePaintBrushSound.Stop();
    }

    public void OnNewKillStreak()
    {
        newKillStreakSound.PlayOneShot(newKillStreakSound.clip); // so they can overlap
    }

    public void OnColorAlternate() // todo :  call when needed
    {
        PlayAudioSourceMusic(colorAlternationMusic);
    }

    public void OnGameStarted()
    {
        _audioSourceSFX.PlayOneShot(gameStartsSound.audioClip, gameStartsSound.volume);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsSceneMenu(scene))
        {
            // todo : voir s'il va y avoir plus qu'une menu scene / plus qu'une toune pour ces menus
            
            if (_isInMenu.HasValue && _isInMenu.Value) return;  // assumant qu'il n'y a qu'une toune pour tous les menus

            _isInMenu = true;
            PlayAudioSourceMusic(backgroundMenuMusic);
        }
        else if (IsSceneEndGame(scene))
        {
            _isInMenu = false;
            PlayAudioSourceMusic(victoryThemeMusic);
        }
        else
        {
            _isInMenu = false;
            PlayAudioSourceMusic(mainTheme);  // todo : voir s'il y a plus qu'une toune de jeu
        }
    }
    
    private void VerifyAudioClipsPresent()
    {
        if (bombExplodedAudio == null)
        {
            throw new Exception($"Audio clip {nameof(bombExplodedAudio)} cannot be null");
        }
        if (bombFusedAudio == null)
        {
            throw new Exception($"Audio clip {nameof(bombFusedAudio)} cannot be null");
        }
        if (buttonClickedAudio.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(buttonClickedAudio)} cannot be null");
        }
        if (backgroundMenuMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(backgroundMenuMusic)} cannot be null");
        }
        if (mainTheme.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(mainTheme)} cannot be null");
        }
        if (victoryThemeMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(victoryThemeMusic)} cannot be null");
        }
        if (pickUpItemSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(pickUpItemSound)} cannot be null");
        }
        if (genericBombEventSound == null)
        {
            throw new Exception($"Audio clip {nameof(genericBombEventSound)} cannot be null");
        }
        if (trampolineSound == null)
        {
            throw new Exception($"Audio clip {nameof(trampolineSound)} cannot be null");
        }
        if (spikeSound == null)
        {
            throw new Exception($"Audio clip {nameof(spikeSound)} cannot be null");
        }
        if (portalSound == null)
        {
            throw new Exception($"Audio clip {nameof(portalSound)} cannot be null");
        } 
        if (bombHitPlayerSound == null)
        {
            throw new Exception($"Audio clip {nameof(bombHitPlayerSound)} cannot be null");
        }
        if (defenseBombSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(defenseBombSound)} cannot be null");
        }
        if (targetBombMovingSound == null)
        {
            throw new Exception($"Audio clip {nameof(targetBombMovingSound)} cannot be null");
        }
        if (acceleratedGameMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(acceleratedGameMusic)} cannot be null");
        }
        if (gameStartsSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(gameStartsSound)} cannot be null");
        }
        if (newKillStreakSound == null)
        {
            throw new Exception($"Audio clip {nameof(newKillStreakSound)} cannot be null");
        }
        if (colorAlternationMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(colorAlternationMusic)} cannot be null");
        }
        if (usePaintBrushSound == null)
        {
            throw new Exception($"Audio clip {nameof(colorAlternationMusic)} cannot be null");
        }
    }
}
