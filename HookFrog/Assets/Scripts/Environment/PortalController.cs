using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PortalController : MonoBehaviour {

	public int targetLevel; 
	private Animator anime;
	ParticleSystem confetti;

	void Awake(){
		confetti = GetComponentInChildren<ParticleSystem>();
		confetti.gameObject.SetActive(false);

		try{
			GameObject transition = GameObject.Find("Level Setup").transform.Find("SceneTransition").gameObject.transform.GetChild(0).gameObject.transform.Find("Swipe-in").gameObject;
			anime = transition.GetComponent(typeof(Animator)) as Animator;
			transition.GetComponent<LevelChanger>().targetLevel = targetLevel;
		}catch{}
	}

	IEnumerator OnTriggerEnter2D(Collider2D other){
		float confettiTime = 1.5f;
		int confettiSpins = 3;
		if(other.tag == "Player") {
			confetti.transform.position = other.transform.position;
			confetti.gameObject.SetActive(true);
			Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
			rb.velocity = Vector2.zero;
			rb.gravityScale = 0;
			PlayerController pc = other.GetComponent<PlayerController>();
			pc.enabled = false;

			Quaternion startRotation = other.transform.rotation;
			float spinTime = confettiTime / (float)confettiSpins;
			Vector3 playerLocation = other.transform.position;
			for(int i = 0; i < confettiSpins; i++){
				for(float t = 0; t < confettiTime / (float)confettiSpins; t++){
					transform.rotation = startRotation * Quaternion.AngleAxis(t / spinTime * 360f * confettiSpins, Vector2.up);
					other.transform.position = playerLocation;

					pc.ResetTongueIfSwinging();

					yield return null;
				}
			}

			yield return new WaitForSeconds(confettiTime / 2);

			if(anime != null){
				anime.SetTrigger("nextLevel");
			}else{
				GameManager.instance.LoadScene(targetLevel);
			}
		}
	}

	void Update(){
		transform.rotation = Quaternion.identity;
	}
}