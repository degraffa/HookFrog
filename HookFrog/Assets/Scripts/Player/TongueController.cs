using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class TongueController : MonoBehaviour {
	[Header("References")]
	// GameObject that marks the point of tongue attachment
	public GameObject tongueHingeAnchor;
	// A reference to the DistanceJoint component on our player,
	// What allows us to have rope-like physics for our tongue
	public DistanceJoint2D tongueJoint;
	public Vector3 worldMousePos;

	public MoveController moveController;
	
	public LineRenderer tongueRenderer;

	[Space]

	[Header("Playtesting variables")]
	public float boostSpeed = 1.65f;
	public float tongueWidth = 0.075f;
	public float shortenSpeed = 5;
	
	public float lengthenSpeed = 5;
	public float minTongueLength = .5f;
	public float maxTongueLength = 20f;

	public float jumpDistanceOnTongueFlick = 5f;

	[Space]

	[Header("Layer Masks")]
	public LayerMask stickLayer;
	public LayerMask environmentLayer;

	bool tongueDistanceSet;
	public bool tongueAttached;
	public float timeAttached;

	List<Vector2> tonguePositions = new List<Vector2>();	
	Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>();
	Vector2 aimDirection;
	Rigidbody2D tongueHingeAnchorRb;
	Rigidbody2D playerRb;
	SpriteRenderer tongueHingeAnchorSprite;

	Collider2D tongueAnchorCollider; 
	

	void Awake(){
		tongueJoint.enabled = false;
		
		tongueHingeAnchorRb = tongueHingeAnchor.GetComponent<Rigidbody2D>();
		playerRb = GetComponent<Rigidbody2D>();

		tongueHingeAnchorSprite = tongueHingeAnchor.GetComponent<SpriteRenderer>();

		tongueRenderer.startWidth = tongueWidth;
		float endWidthOffset = tongueWidth / 5f;
		// the tongue is slightly larger at the end than at the start
		tongueRenderer.endWidth = tongueWidth + endWidthOffset;
	}

	void Update(){
		UpdateCrosshair();
		UpdateTongueRenderer();

		if(tongueAttached){
			timeAttached += Time.deltaTime;
		} else {
			timeAttached = 0;
		}
	}

	public void UpdateCrosshair() {
		Vector2 facingDirection = worldMousePos - playerRb.transform.position;
		float aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);

		// keeps aiming angle posiitve
		if(aimAngle < 0){
			aimAngle = Mathf.PI * 2 + aimAngle;
		}

		// provides a rotation by aimAngle * Mathf.Rad2Deg degrees on the z axis. 
		aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;

		if(!tongueAttached){
			moveController.isSwinging = false;
		} else {

			moveController.isSwinging = true;
			moveController.tongueHook = tonguePositions.Last();

			CheckTongueIntersections();
		}
	}

	void CheckTongueIntersections(){
		if(tonguePositions.Count > 0){
			// What's the last thing we attached to
			Vector3 lastTonguePoint = tonguePositions.Last();

			float raycastLengthOffset = -.1f;

			// Draw a line from the last thing we connected to to the player
			// if that line intersects a collider, then we should
			// and this will be truthy
			// if it doesn't, it will return falsey and we should attach to
			// the new point
			RaycastHit2D playerToCurrentNextHit = Physics2D.Raycast(
				transform.position, 
				(lastTonguePoint - transform.position).normalized, 
				Vector2.Distance(transform.position, lastTonguePoint) + raycastLengthOffset,
				stickLayer | environmentLayer
			);

			// //TODO: Implement this better
			// if(playerToCurrentNextHit) {
			// 	// cast collision to polygonCollider
			// 	PolygonCollider2D colliderWithVertices = playerToCurrentNextHit.collider as PolygonCollider2D;

			// 	if(colliderWithVertices != null){
			// 		// Get the nearest intersection
			// 		Vector2 closestPointToHit = GetClosestColliderPoint(playerToCurrentNextHit, colliderWithVertices);

			// 		// if we've already attached to this point
			// 		if(wrapPointsLookup.ContainsKey(closestPointToHit)){
			// 			wrapPointsLookup.Remove(closestPointToHit);
			// 		} 

			// 		// otherwise..
			// 		tonguePositions.Add(closestPointToHit);
			// 		wrapPointsLookup.Add(closestPointToHit, 0);
			// 		tongueDistanceSet = false;
			// 	}
			// }
		}
	}

	// track collision points on objects so that we can attach to them automatically
	Vector2 GetClosestColliderPoint(RaycastHit2D hit, PolygonCollider2D polyCollider){
		// creates a Dictionary of collision points, and their distance from the Raycast from the player 
		Dictionary<float, Vector2> distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
        position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)), 
        position => polyCollider.transform.TransformPoint(position));

		// order the dictionary by distance
		var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);

		// if our dictionary is nonempty then return the first one, otherwise return zero
		return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
	}

	void FixVelocity(){
		Vector2 oldVelocity = playerRb.velocity;

		Vector2 newDir;

		Vector2 playerToTongueJoint = new Vector2(
			transform.position.x - tongueJoint.transform.position.x,
			transform.position.y - tongueJoint.transform.position.y
		).normalized;

		Vector2 leftDirection = new Vector2(-playerToTongueJoint.x, playerToTongueJoint.y);
		Vector2 rightDirection = new Vector2(playerToTongueJoint.x, -playerToTongueJoint.y);

		if(Input.GetKey(KeyCode.A)){
			newDir = leftDirection;
		} else if(Input.GetKey(KeyCode.D)){
			newDir = rightDirection;
		} else {
			newDir = (playerRb.velocity.x < 0) ? leftDirection : rightDirection;
		}

		playerRb.velocity = newDir * playerRb.velocity.magnitude;
	}

	public void FlickTongue(){
		if(!tongueAttached) {
			tongueRenderer.enabled = true;

			float slightlyLongerThanMaxLength = maxTongueLength + 5;
			RaycastHit2D stickHit = Physics2D.Raycast(transform.position, aimDirection, slightlyLongerThanMaxLength, stickLayer);

			if(stickHit.collider != null) {
				tongueAttached = true;
				moveController.isSwinging = true;

				if(!tonguePositions.Contains(stickHit.point)) {
					tonguePositions.Add(stickHit.point);
					
					float tongueDistanceOffset = (moveController.groundCheck) ? -jumpDistanceOnTongueFlick : 0;
					tongueJoint.distance = Vector2.Distance(transform.position, stickHit.point) + tongueDistanceOffset;
					

					Vector2 playerToJoint = new Vector2(
						playerRb.transform.position.x - tonguePositions.Last().x,
						playerRb.transform.position.x - tonguePositions.Last().y				
					).normalized;
					
					
					FixVelocity();

					tongueJoint.enabled = true;
					tongueHingeAnchorSprite.enabled = true;
				}
			} else {
				tongueRenderer.enabled = false; 
				tongueAttached = false; 
				tongueJoint.enabled = false;
			}
		}	
	}

	// Handles joint points for the tongue. We could have a lot if we wrap around a complex collider
	public void UpdateTongueRenderer() {
		if(!tongueAttached) return;

		playerRb.velocity = new Vector2(playerRb.velocity.x, playerRb.velocity.y);

		tongueRenderer.positionCount = tonguePositions.Count + 1;

		for(int i = tongueRenderer.positionCount - 1; i >= 0; i--) {
			// if this isn't the last point of the line renderer
			if(i != tongueRenderer.positionCount -1){

				tongueRenderer.SetPosition(i, tonguePositions[i]);
				
				// if we're on the second to last one, or if we don't have any extra positions
				if(i == tonguePositions.Count - 1 || tonguePositions.Count == 1) {
					// Store a reference to that one
					Vector2 tonguePosition = tonguePositions[tonguePositions.Count - 1];
					
					// change the anchor's position
					tongueHingeAnchorRb.transform.position = tonguePosition;

					if(!tongueDistanceSet) {
							tongueJoint.distance = Vector2.Distance(transform.position, tonguePosition);
							tongueDistanceSet = true;
					}
				} else if(i - 1 == tonguePositions.IndexOf(tonguePositions.Last())){
					Vector2 tonguePosition = tonguePositions.Last();

					tongueHingeAnchorRb.transform.position = tonguePosition;
					if(!tongueDistanceSet){
						tongueJoint.distance = Vector2.Distance(transform.position, tonguePosition);
						tongueDistanceSet = true;
					}
				}
			} else {
				tongueRenderer.SetPosition(i, transform.position);
			}
		}
	}


	// Moves the player down the tongue by lengthening the tongue joint
	public void LengthenTongue() {
		// our ray should check for things just outside of the character,
		// since we cast from the center we want it to be at least hte scale / 2, 
		// then + .1f to check a little further
		float raycastLength = transform.localScale.y / 2 + 0.1f;

		RaycastHit2D hit = Physics2D.Raycast(
			playerRb.transform.position,
			Vector2.down, 
			raycastLength, 
			environmentLayer
		);

		if(tongueAttached && !hit && tongueJoint.distance <= maxTongueLength) {
			tongueJoint.distance += Time.deltaTime * lengthenSpeed;
		}
	}

	// Moves the player up the tongue by shortening the length of the tongue joint
	public void ShortenTongue(){
		float raycastLength = transform.localScale.y / 2 + 0.1f;

		RaycastHit2D hit = Physics2D.Raycast(
			playerRb.transform.position,
			Vector2.up, 
			raycastLength, 
			environmentLayer
		);

		if(tongueAttached && !hit && tongueJoint.distance >= minTongueLength){
			tongueJoint.distance -= Time.deltaTime * shortenSpeed;
		}
	}

	
	// Detaches tongue from it's attach point and stops drawing it
	public void ResetTongue(){
		tongueJoint.distance = 0;

		tongueJoint.enabled = false;
		tongueAttached = false;

		tongueRenderer.positionCount = 2;
		tongueRenderer.SetPosition(0, transform.position);
		tongueRenderer.SetPosition(1, transform.position);

		tonguePositions.Clear();

		tongueHingeAnchorSprite.enabled = false;

		moveController.isSwinging = false;
		moveController.tongueHook = Vector2.zero;

		wrapPointsLookup.Clear();

		float minimumTimeAttached = 0.5f;
		boostSpeed = (timeAttached > minimumTimeAttached) ? boostSpeed : 1f;
		playerRb.velocity = boostSpeed * (new Vector2(playerRb.velocity.x, playerRb.velocity.y));
	}
}
