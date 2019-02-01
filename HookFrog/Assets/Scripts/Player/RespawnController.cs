using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Death Controller, controls when our frog character should die
// https://i.imgur.com/rGPBGus.png

public class RespawnController : MonoBehaviour {
	public Transform levelStart;

	TongueController tongueController;
	Rigidbody2D rb;

	void Awake() { 
		rb  = GetComponent<Rigidbody2D>();
		tongueController = GetComponent<TongueController>();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		// TODO: Refactor this into a seperate hazard script
		if(other.gameObject.tag == "Hazard" ){
			GameManager.instance.RespawnPlayer("Hazard");
		}
	}

	public void Respawn(){
		rb.velocity = Vector2.zero;
		tongueController.ResetTongue();

		var checkpoint = GameManager.instance.CurrentCheckpoint;
		if(checkpoint == null){
			transform.position = levelStart.position;
			return; 
		}
		transform.position = checkpoint.transform.position;
	}
}
