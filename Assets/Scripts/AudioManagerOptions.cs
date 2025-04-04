using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManagerOptions : MonoBehaviour
{
    public Sound[] sounds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake ()
    {
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }    
    }

    private void Start()
    {
        Debug.Log("HI)")
        Play("MenuSelect");
    }
    public void Play(string name) {
      Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }
}
