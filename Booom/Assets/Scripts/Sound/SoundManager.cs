using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct Audio
{
    public AudioClip audioClip;
    [Range(0,1)]
    public float volume;
}

[Serializable]
public struct Music
{
    public Audio audio;
    public double loopStartTime;
    public double loopEndTime;
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(LoopableMusic))]
public class SoundManager : MonoBehaviour
{
    [Header("--- Menu sounds ---")]
    [SerializeField] 
    private Audio buttonClickedAudio;
    [SerializeField] 
    private Music mainTheme;
    [SerializeField] 
    private List<Audio> playerJoinedSound;
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
    private List<Audio> gameStartsSounds;
    [SerializeField] 
    private Audio battleMusic1;
    [SerializeField] 
    private Audio battleMusic2;
    [SerializeField] 
    private Audio battleMusic3;
    [SerializeField] 
    private Audio battleMusic4;
    [SerializeField] 
    private Audio battleMusic5;
    [SerializeField] 
    private Music tutorialMusic;
    [SerializeField] 
    private Audio colorAlternationMusic;
    [SerializeField]
    private Audio victoryThemeMusic;
    [Space(3)]

    private AudioSource _audioSourceSFX;
    private AudioSource _audioSourceMusic;

    private static bool? _isInMenu = null;

    private LoopableMusic _loopableMusic;

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

        _loopableMusic = GetComponent<LoopableMusic>();

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

    private void PlayAudioSourceMusic(in Music music)
    {
        _loopableMusic.StopMusic();
        _loopableMusic.PlayMusic(music);
    }

    private void PlayAudioSourceMusic(in Audio audio)
    {
        _loopableMusic.StopMusic();
        _audioSourceMusic.clip = audio.audioClip;
        _audioSourceMusic.volume = audio.volume;
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

    public void OnUsePaintBrush(bool isUsing)
    {
        if(isUsing) usePaintBrushSound.Play();
        else usePaintBrushSound.Stop();
    }

    public void OnNewKillStreak()
    {
        newKillStreakSound.PlayOneShot(newKillStreakSound.clip);
    }

    public void OnColorAlternate()
    {
        PlayAudioSourceMusic(colorAlternationMusic);
    }

    public void OnGameStarted(int second)
    {
        _audioSourceSFX.PlayOneShot(gameStartsSounds[second].audioClip, gameStartsSounds[second].volume);
    }

    public void OnPlayerJoined(in PlayerEnum player)
    {
        int index = (int)player - 1;
        if(playerJoinedSound != null && playerJoinedSound.Count > index)
            _audioSourceSFX.PlayOneShot(playerJoinedSound[index].audioClip, playerJoinedSound[index].volume);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsSceneMenu(scene))
        {
            if (_isInMenu.HasValue && _isInMenu.Value) return;

            _isInMenu = true;
            PlayAudioSourceMusic(mainTheme);
        }
        else if (IsSceneEndGame(scene))
        {
            _isInMenu = false;
            PlayAudioSourceMusic(victoryThemeMusic);
        }
        else
        {
            int i = scene.buildIndex;
            Audio? music = null;
            if(RoundManager.MapsToPlay[0] == i) music = battleMusic1;
            else if(RoundManager.MapsToPlay[1] == i) music = battleMusic2;
            else if(RoundManager.MapsToPlay[2] == i) music = battleMusic3;
            else if(RoundManager.MapsToPlay[3] == i) music = battleMusic4;
            else if(RoundManager.MapsToPlay[4] == i) music = battleMusic5;
            
            if(!music.HasValue) PlayAudioSourceMusic(tutorialMusic);
            else PlayAudioSourceMusic(music.Value);

            _isInMenu = false;
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
        if (mainTheme.audio.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(mainTheme)} cannot be null");
        }
        if (battleMusic1.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(battleMusic1)} cannot be null");
        }
        if (battleMusic2.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(battleMusic1)} cannot be null");
        }
        if (battleMusic3.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(battleMusic1)} cannot be null");
        }
        if (battleMusic4.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(battleMusic1)} cannot be null");
        }
        if (battleMusic5.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(battleMusic1)} cannot be null");
        }
        if (tutorialMusic.audio.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(tutorialMusic)} cannot be null");
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
        if (gameStartsSounds == null)
        {
            throw new Exception($"Audio clip {nameof(gameStartsSounds)} cannot be null");
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
