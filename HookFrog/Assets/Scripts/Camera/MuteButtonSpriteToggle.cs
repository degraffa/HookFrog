using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButtonSpriteToggle : MonoBehaviour {

	private Image targetImage;
	public Sprite muted;
	public Sprite unmuted;
	private AudioManager audioManager;

	private Button button;

	public void Initalize(bool value){
	}

	void Awake (){
		
		button = GetComponent<Button>();
		targetImage = button.GetComponent<Image>();
		
	}

	void Start (){
		targetImage.sprite = AudioManager.instance.isMuted ? muted : unmuted;
	}

	public void toggleSprite(){
		muteAudio();

		targetImage.sprite = AudioManager.instance.isMuted ? muted : unmuted;
	}

	public void muteAudio(){
		AudioManager.instance.toggleMute();
	}
}
