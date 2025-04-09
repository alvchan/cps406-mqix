using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    [SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] Slider slider;

    private void Start()
    {
        slider.value = AudioManager.Instance.sliderValue;
    }
    public void SliderVolume(Slider slider)
    {
        float sliderValue = slider.value;
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f; // Decibel Scale for Amplitude formula https://blog.demofox.org/2015/04/14/decibels-db-and-amplitude/
        mixerGroup.audioMixer.SetFloat("Master", dB);
        AudioManager.Instance.sliderValue = sliderValue;
    }
}
