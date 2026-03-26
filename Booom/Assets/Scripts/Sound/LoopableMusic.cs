using UnityEngine;

public class LoopableMusic : MonoBehaviour
{
    private int _loopStartSamples;
    private int _loopEndSamples;
    private int _loopLengthSamples;
    
    private AudioSource _audioSource;

    private bool _isMusicPlaying = false;

    private void Start()
    {
        _audioSource = GetComponents<AudioSource>()[1];
    }

    public void PlayMusic(in Music music)
    {
        _audioSource.clip = music.audio.audioClip;
        _audioSource.volume = music.audio.volume;
        
        var frequency = music.audio.audioClip.frequency;
        _loopStartSamples = (int)(music.loopStartTime * frequency);
        _loopEndSamples = (int)(music.loopEndTime * frequency);
        _loopLengthSamples = _loopEndSamples - _loopStartSamples;

        _isMusicPlaying = true;
        _audioSource.Play();
    }

    public void StopMusic()
    {
        _isMusicPlaying = false;
    }

    private void Update()
    {
        if (_isMusicPlaying && _audioSource.timeSamples >= _loopEndSamples)
        {
            _audioSource.timeSamples -= _loopLengthSamples;
        }
    }
}
