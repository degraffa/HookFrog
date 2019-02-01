using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour {

    public GameObject player;
    Vector3 playerPos;
    Collider2D playerCollider;
	public float lerpSpeed = .05f;

    FocusBox focusBox;
    public float focusBoxWidth = 2f;
    public float focusBoxHeight = 3f;

	public float mapLowerXBound = -5000f;
	public float mapLowerYBound = -5000f;
    public Vector2 offset = new Vector2(0, 5.5f);

    struct FocusBox {
        public Vector2 position;
        public float up, down, left, right;

        public FocusBox(Vector2 initPos, float w, float h) {
            position = initPos;
            up = initPos.y + h;
            down = initPos.y - h;
            left = initPos.x - w;
            right = initPos.x + w;
        }

    }

    void Start(){
        focusBox = new FocusBox(playerPos + new Vector3(1,0,0), focusBoxWidth, focusBoxHeight);
        playerCollider = player.GetComponent<Collider2D>();
        playerPos = player.transform.position;
    }

    void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(focusBox.position, new Vector2(focusBoxWidth*2f, focusBoxHeight*2f));
    }

    // Update is called once per frame
    void Update () {
        if (Mathf.Abs(playerPos.x - player.transform.position.x) > focusBoxWidth * 2f 
            || Mathf.Abs(playerPos.y - player.transform.position.y) > focusBoxWidth * 2f) {
            playerPos = player.transform.position;
            Camera.main.transform.position = new Vector3(playerPos.x, playerPos.y, -20);
            focusBox = new FocusBox(new Vector2(playerPos.x, playerPos.y+1), focusBoxWidth, focusBoxHeight);
        }
        else {
            playerPos = player.transform.position;

            float deltaX = 0f;
            if (playerPos.x - playerCollider.bounds.size.x < focusBox.left) {
                deltaX = playerPos.x - playerCollider.bounds.size.x - focusBox.left;
            }
            else if (playerPos.x + playerCollider.bounds.size.x > focusBox.right) {
                deltaX = playerPos.x + playerCollider.bounds.size.x - focusBox.right;
            }
            focusBox.left += deltaX;
            focusBox.right += deltaX;

            float deltaY = 0f;
            if (playerPos.y - playerCollider.bounds.size.y < focusBox.down) {
                deltaY = playerPos.y - playerCollider.bounds.size.y - focusBox.down;
            }
            else if (playerPos.y + playerCollider.bounds.size.y > focusBox.up) {
                deltaY = playerPos.y + playerCollider.bounds.size.y - focusBox.up;
            }

            focusBox.up += deltaY;
            focusBox.down += deltaY;
            focusBox.position = new Vector2((focusBox.left + focusBox.right) / 2, (focusBox.up + focusBox.down) / 2);

            Vector2 lerp = Vector2.Lerp(Camera.main.transform.position, focusBox.position + offset, lerpSpeed);
            
            Vector2 cameraPosition = new Vector2(Mathf.Max(lerp.x, mapLowerXBound), Mathf.Max(lerp.y, mapLowerYBound));
            Camera.main.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -30);
        }
	}
}
