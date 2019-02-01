using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

	public int numOfFlashes = 3;

	public float warningTime = 5f;
	public float betweenTime = .5f;
	public float scaleFireTime = .1f;
	public float fireTime = 2f;
	public float fadeTime = 1f;

	public GameObject warningLaser;
	public GameObject actualLaser;

	SpriteRenderer actualLaserSpriteRenderer;

	BoxCollider2D actualLaserCollider;

	IEnumerator shootLaser;

	float angle = 0;

	void Start(){
		// get Components/objects
		actualLaserSpriteRenderer = actualLaser.GetComponent<SpriteRenderer>();
		actualLaserCollider = actualLaser.GetComponent<BoxCollider2D>();

		shootLaser = ShootLaser();

		//disable lasers
		warningLaser.SetActive(false);
		actualLaser.SetActive(false);

		actualLaserCollider.enabled = false;

		transform.Rotate(0, 0, angle);

		StartCoroutine(shootLaser);
	}

	IEnumerator ShootLaser() {
		// Fire the warning

		for(int i = 0; i < numOfFlashes * 2; i++) {
			if(i % 2 == 0){
				warningLaser.SetActive(true);

				// Play warning audio
				AudioManager.instance.Play("warning");
			} else {
				warningLaser.SetActive(false);
			}

			yield return new WaitForSeconds(warningTime / (numOfFlashes * 2));
		}

		warningLaser.SetActive(false);

		// wait for between time
		yield return new WaitForSeconds(betweenTime);

		// Fire laser
		actualLaser.SetActive(true);
		actualLaserCollider.enabled = true;

		// Play laser audio
		AudioManager.instance.Play("laser");

		// while firing, scale it up slowly
		Vector2 targetScale = actualLaser.transform.localScale;

		for(float currentTime = 0; currentTime <= scaleFireTime; currentTime += Time.deltaTime){
			float currentYScale = Mathf.Lerp(
				0,
				targetScale.y,
				currentTime / scaleFireTime
			);

			actualLaser.transform.localScale = new Vector2(actualLaser.transform.localScale.x, currentYScale);

			yield return null;
		}

		// keep it at that scale, shake up and down a little
		for(int i = 0; i < 60; i++){
			float shakeAmount = .3f;
			if(i % 2 == 1){
				shakeAmount = -shakeAmount;
			}

			Vector2 shakePos = new Vector2(actualLaser.transform.position.x, actualLaser.transform.position.y + shakeAmount);
			actualLaser.transform.position = shakePos;

			yield return new WaitForSeconds(fireTime / 60);
		}

		// fade
		actualLaserCollider.enabled = false;
		Vector2 originalScale = transform.localScale;
		for (float f = 1f; f >= 0; f -= 0.1f) {
			Color c = actualLaserSpriteRenderer.color;
			c.a = f;
			actualLaserSpriteRenderer.color = c;

			yield return new WaitForSeconds(fadeTime / 10);
    	}


		actualLaser.SetActive(false);

		Destroy(this.gameObject);
	}

}
