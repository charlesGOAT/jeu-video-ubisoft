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
    private Audio bombFusedAudio;
    [SerializeField] 
    private Audio bombExplodedAudio;
    [SerializeField] 
    private Audio genericBombEventSound;
    [Space(3)]
    
    [Header("--- Item sounds ---")]
    [SerializeField] 
    private Audio pickUpItemSound;
    [Space(3)]
    
    [Header("--- Movement sounds ---")]
    [SerializeField] 
    private Audio trampolineSound;
    [SerializeField]
    private Audio spikeSound;
    [SerializeField] 
    private Audio portalSound;
    [Space(3)]

    [Header("--- Game music ---")]
    [SerializeField] 
    private Audio gameMusic;
    [SerializeField]
    private Audio endGameMusic;
    [Space(3)]

    private AudioSource _audioSourceSFX;
    private AudioSource _audioSourceMusic;

    private static bool? _isInMenu = null;

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
            throw new Exception($"Audio clip {nameof(bombExplodedAudio)} cannot be null");
        }
        if (bombFusedAudio.audioClip == null)
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
        if (gameMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(gameMusic)} cannot be null");
        }
        if (endGameMusic.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(endGameMusic)} cannot be null");
        }
        if (pickUpItemSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(pickUpItemSound)} cannot be null");
        }
        if (genericBombEventSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(genericBombEventSound)} cannot be null");
        }
        if (trampolineSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(trampolineSound)} cannot be null");
        }
        if (spikeSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(spikeSound)} cannot be null");
        }
        if (portalSound.audioClip == null)
        {
            throw new Exception($"Audio clip {nameof(portalSound)} cannot be null");
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

    public void OnBombEvent()
    {
        _audioSourceSFX.PlayOneShot(genericBombEventSound.audioClip, genericBombEventSound.volume);
    }

    public void OnPickupItem(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(pickUpItemSound.audioClip, position, pickUpItemSound.volume);
    }

    public void OnEnterTrampoline(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(trampolineSound.audioClip, position, trampolineSound.volume);
    }
    
    public void OnEnterPortal(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(portalSound.audioClip, position, portalSound.volume);
    }

    public void OnEnterSpikes(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(spikeSound.audioClip, position, spikeSound.volume);
    }

    public void OnGameEnded()
    {
        PlayAudioSourceMusic(endGameMusic);
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
        else
        {
            _isInMenu = false;
            PlayAudioSourceMusic(gameMusic);  // todo : voir s'il y a plus qu'une toune de jeu
        }
    }
}
