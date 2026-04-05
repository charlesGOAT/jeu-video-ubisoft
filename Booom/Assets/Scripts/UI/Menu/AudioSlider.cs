using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float spinSpeed = 180f;

    private void Start()
    {
        if (slider == null) return;

        if (gameObject.name == "MusicSlider")
        {
            slider.value = SoundManager.Instance.MusicVolume;
        }
        else
        {
            slider.value = SoundManager.Instance.SFXVolume;
        }
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject && handle != null)
        {
            handle.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (gameObject.name == "MusicSlider")
        {
            SoundManager.Instance.SetMusicVolume(value);
        }
        else
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }
}