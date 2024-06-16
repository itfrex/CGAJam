using UnityEngine.Audio;
using System;
using UnityEngine;

/* * * * * * * * * * * * * * * * * * *
 * To call a sound from another script:
 * FindObjectOfType<AudioManager>().Play("sound");
 * * * * * * * * * * * * * * * * * * */

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = s.group;
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return;
        s.source.Play();
    }

    public float GetLength(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return 0;
        return s.clip.length;
    }
    public void PlayDelayed(string name, float delay)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return;
        s.source.PlayScheduled(AudioSettings.dspTime + delay);
    }
    public void Pause(string name, bool paused)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return;
        if (paused)
            s.source.Pause();
        else
            s.source.UnPause();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return;
        if (s.source)
            s.source.Stop();
    }

    public float Time(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return 0f;
        return s.source.time;
    }

    public void ResetTime(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return;
        s.source.time = 0;
    }

    public void TransitionSong(string name1, string name2)
    {
        Sound firstSong = Array.Find(sounds, sound => sound.name == name1);
        Sound secondSong = Array.Find(sounds, sound => sound.name == name2);

        if (firstSong == null && secondSong == null)
            return;

        float transitionTime = firstSong.source.time;
        secondSong.source.time = transitionTime;

        firstSong.source.Stop();
        secondSong.source.Play();
    }
    public AudioClip GetClip(string name){
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        if (s == null)
            return null;
        return s.clip;
    }

}