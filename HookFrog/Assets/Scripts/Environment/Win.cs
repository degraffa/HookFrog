using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour {
	public GameObject winScreen;

	private void OnCollisionEnter2D(Collision2D other) {
		if( other.gameObject.tag == "Player" ){
			winScreen.GetComponent<SpriteRenderer>().enabled = true;
		}
	}
}
