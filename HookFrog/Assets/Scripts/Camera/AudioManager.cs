using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager instance;
	public float globalVolume;
	public bool isMuted;
	public Sound bgMusic;
	public Sound[] sounds;

	void Awake ()
	{
		globalVolume = 0.75f;

		if (instance != null){
			Destroy(gameObject);
			return;
		}
		else{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds){
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;
			s.source.playOnAwake = false;
			if( s.name == "bg" ){
				bgMusic = s;
			}
		}
	}

	public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = isMuted ? 0 : globalVolume;
		s.source.Play();
	}

	public void Stop(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.Stop();
	}

	public void toggleMute(){
		Debug.Log(bgMusic.source);
		if( isMuted ){
			// instance.Play("bg");
			bgMusic.source.Play();

		}else{
			// instance.Stop("bg");
			bgMusic.source.Stop();
		}
		isMuted = !isMuted;
	}

}
