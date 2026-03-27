using UnityEngine;

public class LoopableMusic : MonoBehaviour
{
    private int _loopStartSamples;
    private int _loopEndSamples;
    private int _loopLengthSamples;
    
    [SerializeField]
    private AudioSource audioSource;

    private bool _isMusicPlaying = false;

    private void Start()
    {
        audioSource = GetComponents<AudioSource>()[1];
    }

    public void PlayMusic(in Music music)
    {
        audioSource.clip = music.audio.audioClip;
        audioSource.volume = music.audio.volume;
        
        var frequency = music.audio.audioClip.frequency;
        _loopStartSamples = (int)(music.loopStartTime * frequency);
        _loopEndSamples = (int)(music.loopEndTime * frequency);
        _loopLengthSamples = _loopEndSamples - _loopStartSamples;

        _isMusicPlaying = true;
        audioSource.Play();
    }

    public void StopMusic()
    {
        _isMusicPlaying = false;
    }

    private void Update()
    {
        if (_isMusicPlaying && audioSource.timeSamples >= _loopEndSamples)
        {
            audioSource.timeSamples -= _loopLengthSamples;
        }
    }
}
