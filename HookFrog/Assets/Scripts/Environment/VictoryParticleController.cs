﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryParticleController : MonoBehaviour {

	ParticleSystem particleSystem;

	void Start(){
		particleSystem = GetComponent<ParticleSystem>();

	}
}