using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryScreech : MonoBehaviour {
	private void OnCollisionEnter2D(Collision2D other) {
		// Clear Player Save Data, add that the Player Won the Game
        if(!Constants.IS_DEBUGGING){
            PlayerPrefs.SetInt("Game Completed", 9001);
        }
	}
}
