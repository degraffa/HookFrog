using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

	public Transform respawnPoint;
	public string checkpointName;  

	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Player") {
			GameManager.instance.CurrentCheckpoint = this;
		}
	}

}
