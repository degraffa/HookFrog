using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSpawner : MonoBehaviour {

    public float timeBetweenLasers = 1f;
    public float initialWaitTime = 3f;
    public bool shootConstantly = false;
    public float constantShootTime = .1f;

    public Laser laserPrefab;
    Laser activeLaser;
    PlayerController player;

    public void Start(){
        player = FindObjectOfType<PlayerController>();
    }

    float t = 0;
    public void Update(){
        if(t > initialWaitTime  ){
            if(activeLaser == null && Time.time % timeBetweenLasers >= 0 && Time.time % timeBetweenLasers <= 0.5){
                activeLaser = Instantiate(laserPrefab, this.transform);
                activeLaser.transform.position = new Vector2(player.transform.position.x, player.transform.position.y);
            }
        } else {
            t += Time.deltaTime;
        }
        
    }
}
