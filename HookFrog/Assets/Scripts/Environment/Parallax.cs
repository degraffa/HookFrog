using UnityEngine;
using System.Collections;

public class Parallax : MonoBehaviour {

    public float scrollSpeed = 1f;
	public float smoothing = 0.01f;

    private Vector2 savedOffset;
    private Vector3 savedPos;
	public GameObject playerObj;
    private Transform player;
	private Vector3 prevPlayerPos;
    private float magicYCutoff = 7f;

	void Awake(){
		savedOffset = this.GetComponent<Renderer>().sharedMaterial.GetTextureOffset ("_MainTex");
        savedPos = this.transform.position - this.transform.parent.transform.position;
        //playerObj = GameObject.Find("Player");
		//player = playerObj.transform;
        player = this.transform.parent;
        //if(GameObject.Find("Player") == null) Print.Log("[Parallax]FROG OBJECT SHOULD BE NAMED 'Player'!");
	}

    void Start () {
        prevPlayerPos = player.position;
    }

    void Update () {
        if (Mathf.Pow(prevPlayerPos.x - player.position.x,2f) + Mathf.Pow(prevPlayerPos.y - player.position.y,2f) >= 400f){
            this.transform.position = new Vector3(this.transform.position.x, this.transform.parent.transform.position.y, this.transform.position.z);

            prevPlayerPos = player.position;
            savedOffset = this.GetComponent<Renderer>().sharedMaterial.GetTextureOffset ("_MainTex");
            savedPos = this.transform.position - this.transform.parent.transform.position;
            return;
        }
        float newX = savedOffset.x + (player.position.x - prevPlayerPos.x) * smoothing * scrollSpeed;
        if(newX >= 1){
            newX = 0;
        }else if(newX<=-1){
            newX = 0;
        }
        if((player.position.y - prevPlayerPos.y > 0 && savedPos.y > -magicYCutoff) ||
        (player.position.y - prevPlayerPos.y < 0 && savedPos.y < magicYCutoff)){
            //Print.Log(savedPos);
            float newY = this.transform.position.y - (player.position.y - prevPlayerPos.y) * 0.7f;
            this.transform.position = new Vector3(this.transform.position.x, newY, this.transform.position.z);
        }
        Vector2 offset = new Vector2 (newX, savedOffset.y);
        this.GetComponent<Renderer>().sharedMaterial.SetTextureOffset ("_MainTex", offset);

        prevPlayerPos = player.position;
        savedOffset = this.GetComponent<Renderer>().sharedMaterial.GetTextureOffset ("_MainTex");
        savedPos = this.transform.position - this.transform.parent.transform.position;
    }

    void OnDisable () {
       this.GetComponent<Renderer>().sharedMaterial.SetTextureOffset ("_MainTex", savedOffset);
    }
}