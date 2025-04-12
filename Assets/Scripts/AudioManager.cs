using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;
    public static AudioManager Instance;
    public float sliderValue = 0.5f;
    void Awake ()
    {
        if (Instance == null) // The same AudioManager instance is used through every scene.
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else // Destroy the newly initialized AudioManager when going back to the start menu (Scene 0)
        {
            //Destroy(gameObject);
        }
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.masterGroup;
        }
        
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Play("MenuSong");
            SetVolume("MenuSong", 0.44f);
        }
    }
    public void Play(string name) {
      Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            s.source.Play();
        }   
    }
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Pause();
    }

    public void UnPause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.UnPause();
    }

    public bool isPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s.source.isPlaying;
    }

    public Sound getSound(string name) {
        return Array.Find(sounds, sound => sound.name == name);
    }

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Sound '{name}' not found!");
            return;
        }
        volume = Mathf.Clamp01(volume);
        s.source.volume = volume;
    }

}
