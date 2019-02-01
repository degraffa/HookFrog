using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
[RequireComponent(typeof(TongueController))]
public class PlayerInput : MonoBehaviour {
	MoveController moveController;
	TongueController tongueController;

	float horizontalInput = 0f;
	float verticalInput = 0f;
	bool jump = false;
	bool tongueFlick = false;
	bool resetTongue = false;
	bool reset = false; 
	
    void Awake() {
        moveController = GetComponent<MoveController>();
		tongueController = GetComponent<TongueController>();
    }

	// Update is called once per frame
	void Update () {

		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");

		tongueController.worldMousePos = 
			Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

		if(verticalInput < 0){
			tongueController.LengthenTongue();
		} else if(verticalInput > 0) {
			tongueController.ShortenTongue();
		}

		jump = Input.GetKeyDown(KeyCode.Space);

		if(Input.GetMouseButtonDown(0)) {
			tongueFlick = true;
		}

		if(Input.GetMouseButtonUp(0)) {
			resetTongue = true;
		}

		if(Input.GetButtonDown("Reset")) {
			reset = true; 
		}

		
		moveController.Move(horizontalInput);
	}

	void FixedUpdate() {
		if(jump) {
			Debug.Log("jamp");
			jump = false;
			moveController.Jump();
		}

		if(tongueFlick) {
			tongueFlick = false;
			tongueController.FlickTongue();
		}

		if(resetTongue) {
			resetTongue = false;
			tongueController.ResetTongue();
		}

		if(reset){
			reset = false; 
			GameManager.instance.LoadScene();
		}
	}
}