using System;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
	public static ScoreDisplay instance;

	void Awake()
	{
		if (instance != null){
			Destroy(gameObject);
			return;
		}
		else{
			instance = this;
		}
	}
}
