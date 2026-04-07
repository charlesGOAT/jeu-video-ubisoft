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
    [SerializeField] 
    private Audio transitionSFX;
    
    public AudioSource AudioSourceSFX { get; private set; }
    public AudioSource AudioSourceMusic { get; private set; }

    private static bool? _isInMenu = null;

    private LoopableMusic _loopableMusic;

    private bool IsSceneMenu(in Scene scene) => scene.name.Contains("menu", StringComparison.OrdinalIgnoreCase);
    private bool IsSceneEndGame(in Scene scene) => scene.name.Contains("EndGame", StringComparison.OrdinalIgnoreCase);

    public static SoundManager Instance { get; private set; }
    
    [Range(0f, 1f)]
    private float _musicVolume = 1f;

    private float musicVolume => PlayerPrefs.GetFloat("MusicVolume", 1f);
    [Range(0f, 1f)]
    private float _sfxVolume = 1f;
    private float sfxVolume => PlayerPrefs.GetFloat("SFXVolume", 1f);


    public float MusicVolume => _musicVolume;
    public float SFXVolume => _sfxVolume;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SetMusicVolume(_musicVolume);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        SetSFXVolume(_sfxVolume);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _loopableMusic = GetComponent<LoopableMusic>();

        InitializeAudioSources();
        VerifyAudioClipsPresent();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        AudioSourceMusic.volume = _musicVolume;
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);

       // // if (AudioSourceMusic != null) AudioSourceMusic.volume *= _musicVolume;
       //
       //  battleMusic1.volume *= _musicVolume;
       //  battleMusic2.volume *= _musicVolume;
       //  battleMusic3.volume *= _musicVolume;
       //  battleMusic4.volume *= _musicVolume;
       //  battleMusic5.volume *= _musicVolume;
       //  tutorialMusic.audio.volume *= _musicVolume;
       //  colorAlternationMusic.volume *= _musicVolume;
       //  victoryThemeMusic.volume *= _musicVolume;
       //  mainTheme.audio.volume *= _musicVolume;
       //
       //  if (gameStartsSounds != null)
       //  {
       //      for (int i = 0; i < gameStartsSounds.Count; i++)
       //      {
       //          Audio sound = gameStartsSounds[i]; 
       //          sound.volume *= _musicVolume; 
       //          gameStartsSounds[i] = sound; 
       //      }
       //  }
       //  
       //  if (playerJoinedSound != null)
       //  {
       //      for (int i = 0; i < playerJoinedSound.Count; i++)
       //      {
       //          Audio sound = playerJoinedSound[i]; 
       //          sound.volume *= _musicVolume; 
       //          playerJoinedSound[i] = sound; 
       //      }
       //  }

        PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);

        // if (AudioSourceSFX != null) AudioSourceSFX.volume *= _sfxVolume;
        //
        // bombFusedAudio.volume *= _sfxVolume;
        // bombExplodedAudio.volume *= _sfxVolume;
        // genericBombEventSound.volume *= _sfxVolume;
        // targetBombMovingSound.volume *= _sfxVolume;
        // trampolineSound.volume *= _sfxVolume;
        // spikeSound.volume *= _sfxVolume;
        // portalSound.volume *= _sfxVolume;
        // bombHitPlayerSound.volume *= _sfxVolume;
        // newKillStreakSound.volume *= _sfxVolume;
        // usePaintBrushSound.volume *= _sfxVolume;
        // transitionSFX.volume *= _sfxVolume;
        //
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        PlayerPrefs.Save();
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
        AudioSourceSFX = audioSources[0];
        AudioSourceMusic = audioSources[1];
        AudioSourceMusic.loop = true;
    }

    private void PlayAudioSourceMusic(Music music)
    {
        _loopableMusic.StopMusic();
        _loopableMusic.PlayMusic(music);
    }

    private void PlayAudioSourceMusic(in Audio audio)
    {
        _loopableMusic.StopMusic();
        AudioSourceMusic.clip = audio.audioClip;
        AudioSourceMusic.volume = audio.volume * musicVolume;
        AudioSourceMusic.Play();
    }

    public void OnBombFused()
    {
        bombFusedAudio.volume = sfxVolume;
        bombFusedAudio.Play();
    }

    public void OnBombExploded()
    {
        bombExplodedAudio.volume = sfxVolume;
        bombExplodedAudio.Play();
    }

    public void OnMenuButtonPressed()
    {
        if (AudioSourceSFX == null) return;
        AudioSourceSFX.PlayOneShot(buttonClickedAudio.audioClip, buttonClickedAudio.volume * sfxVolume);
    }

    public void OnBombEvent()
    {
        genericBombEventSound.volume = sfxVolume;
        genericBombEventSound.Play();
    }

    public void OnPickupItem()
    {
        AudioSourceSFX.PlayOneShot(pickUpItemSound.audioClip, pickUpItemSound.volume * sfxVolume);
    }

    public void OnEnterTrampoline()
    {
        trampolineSound.volume = sfxVolume;
        trampolineSound.Play();
    }
    
    public void OnEnterPortal()
    {
        portalSound.volume = sfxVolume;
        portalSound.Play();
    }

    public void OnEnterSpikes()
    {
        spikeSound.volume = sfxVolume;
        spikeSound.Play();
    }

    public void OnPlayerHitByBomb()
    {
        bombHitPlayerSound.volume = sfxVolume;
        bombHitPlayerSound.Play();
    }

    public void OnTargetBombMoving(bool isMoving)
    {
        targetBombMovingSound.volume = sfxVolume;
        if(isMoving) targetBombMovingSound.Play();
        else targetBombMovingSound.Stop();
    }

    public void OnDefenseBombExploded()
    {
        AudioSourceSFX.PlayOneShot(defenseBombSound.audioClip, defenseBombSound.volume * sfxVolume);
    }

    public void OnUsePaintBrush(bool isUsing)
    {
        usePaintBrushSound.volume = sfxVolume;
        if(isUsing) usePaintBrushSound.Play();
        else usePaintBrushSound.Stop();
    }

    public void OnNewKillStreak()
    {
        newKillStreakSound.volume = sfxVolume;
        newKillStreakSound.Play();
    }

    public void OnColorAlternate()
    {
        PlayAudioSourceMusic(colorAlternationMusic);
    }

    public void OnGameStarted(int second)
    {
        AudioSourceSFX.PlayOneShot(gameStartsSounds[second].audioClip, gameStartsSounds[second].volume * sfxVolume);
    }

    public void OnPlayerJoined(in PlayerEnum player)
    {
        int index = (int)player - 1;
        if(playerJoinedSound != null && playerJoinedSound.Count > index)
            AudioSourceSFX.PlayOneShot(playerJoinedSound[index].audioClip, playerJoinedSound[index].volume * sfxVolume);
    }

    public void OnSceneTransition()
    {
        AudioSourceSFX.PlayOneShot(transitionSFX.audioClip, transitionSFX.volume * sfxVolume);
    }

    public void PlayBattleMucic()
    {
        int i = SceneManager.GetActiveScene().buildIndex;
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