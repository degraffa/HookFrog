using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TongueController))]
public class TongueUI : MonoBehaviour {
	[Header("Sprites")]
    public GameObject circle;
	public GameObject crosshair;

	[Space][Header("Playtesting")]
	float circleSizeDecreaseFactor = .75f;
	float crosshairScale = 5f;

    TongueController tongueController;
    Rigidbody2D playerRb;
    Vector2 playerPos;
    Vector3 worldMousePos;
    Vector2 endpt;

    List<GameObject> UIcircles = new List<GameObject>();
    float maxLength;
    int count;

	// Use this for initialization
	void Start () {
		tongueController = GetComponent<TongueController>();
		maxLength = tongueController.maxTongueLength;
		count = Mathf.CeilToInt(maxLength/2);
		playerRb = GetComponent<Rigidbody2D>();
		playerPos = new Vector2(playerRb.transform.position.x, playerRb.transform.position.y);

		crosshair.transform.localScale = new Vector2(crosshairScale, crosshairScale);
	}
	
	// Update is called once per frame
	void Update ()
    {
        Circles();
    }

    private void Circles()
    {
        if (tongueController.tongueAttached)
        {
            FlushCircles();
        }
        else
        {
            worldMousePos = tongueController.worldMousePos;
            playerPos = new Vector2(playerRb.transform.position.x, playerRb.transform.position.y);

            Vector2 facingDirection = worldMousePos - playerRb.transform.position;
            facingDirection = facingDirection.normalized;
            endpt = new Vector2(playerPos.x + facingDirection.x * maxLength, playerPos.y + facingDirection.y * maxLength);

            float slightlyLongerThanMaxLength = maxLength + 5;

            RaycastHit2D stickHit = Physics2D.Raycast(
					transform.position, 
					facingDirection, 
					slightlyLongerThanMaxLength, 
					tongueController.stickLayer
				);

            RaycastHit2D bothHit = Physics2D.Raycast(
					transform.position, 
					facingDirection, 
					slightlyLongerThanMaxLength, 
					tongueController.environmentLayer | tongueController.stickLayer
				);

            if (stickHit.collider != null)
            {
                crosshair.transform.position = stickHit.point;
            }
            else
            {
                crosshair.transform.position = new Vector2(-200000, 200000);
            }

            DrawCircles(bothHit);
        }
    }

    private void DrawCircles(RaycastHit2D hit)
    {
		//first, flush the previous set of circles
		FlushCircles();

        for (int i = 0; i < count; i++) {
            Vector2 circlePos = Vector2.Lerp(playerPos, endpt, (float)(i + 1) / count);
            float playerToCircleDistance = Vector2.Distance(transform.position, circlePos);
            
            float playerToHitPointDistance = Vector2.Distance(transform.position, hit.point);

            if (hit.collider == null || playerToCircleDistance < playerToHitPointDistance)
            {
                UIcircles.Add(Instantiate(circle, transform));
                UIcircles[i].transform.position = new Vector2(circlePos.x, circlePos.y);

                float size = (count - i * circleSizeDecreaseFactor) / count;
                UIcircles[i].transform.localScale = new Vector2(size, size);
            }
        }
    }

	void FlushCircles(){
		for (int i = 0; i < UIcircles.Count; i++)
            {
                Destroy(UIcircles[i]);
            }
            UIcircles = new List<GameObject>();
	}
}
