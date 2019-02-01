using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Player"){
			GameManager.instance.RespawnPlayer("Death Zone"); 
		}
	}
}
