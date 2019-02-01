using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanController : MonoBehaviour {

	public float hoverForce = 75; 

	void OnTriggerStay2D(Collider2D other){
		var rigid = other.GetComponent<Rigidbody2D>(); 
		rigid.AddForce(Vector3.up * hoverForce);
	}
}
