using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChanger : MonoBehaviour {

	public int targetLevel;
	public void TransitionComplete(){
		if(targetLevel != null){
			GameManager.instance.LoadScene(targetLevel);
		}
	}
}
