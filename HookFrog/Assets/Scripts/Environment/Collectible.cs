using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Player") {
			GameManager.instance.addCollectable(this); 
			Destroy(gameObject); 
			Debug.Log("Collectible collected");
		}
	}

}
