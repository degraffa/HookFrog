using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class PlayerController : MonoBehaviour {

	const float epsilon = 1e-6f;
	// STATE PATTERN
	// http://gameprogrammingpatterns.com/state.html
	// ------------------------------------------------
	public GameObject marker;
	public Animator animator;

	public LineRenderer tongueRenderer;
	public LayerMask tongueLayerMask;
	public LayerMask environmentLayerMask;
	public DistanceJoint2D tongueJoint;
	public GameObject tongueHingeAnchor;
	public float swingForce = 8.5f;
	public float swingABMultiplier = 1.4f;
	public float moveSpeed 	= 1.0f;
	public float boostSpeed = 1.25f;
	public float tongueSpeed = 5;
	public bool groundCheck;
	public bool walkEnabled;
	public int abValue;
	public PlayerState currentState;
	private CircleCollider2D playerCollider;
	private SpriteRenderer playerSprite;
	private Rigidbody2D playerRb;

	private Color defaultColor;
	private float defaultGravityScale;

	public interface PlayerState {
		void Update();
		void FixedUpdate();
	}


	public bool GroundCheck(){

		int nRays = 10; // cast 10 rays over the bottom 120 deg
		float dAngle = Mathf.PI/nRays; // delta angle

		groundCheck = false;
		bool gcCast = false;

		float LDiagAngle = 7.0f*Mathf.PI/6.0f;
		float angle;
		for( int i = 0; i < nRays; i++ ){
			angle = LDiagAngle + i*dAngle;
			gcCast = GroundCheck(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
			groundCheck = groundCheck || gcCast; // can break as soon as true but will do all raycasts for now
		}
		return groundCheck;
	}

	public bool GroundCheck(Vector2 direction){

		float halfHeight = transform.GetComponent<SpriteRenderer>().bounds.extents.y;

		float groundDistanceOffset = 0.05f;
        float groundCheckDistance = playerCollider.radius + groundDistanceOffset;

        // we want our raycast origin to be at the bottom of our character, minus a little bit
        float raycastOriginOffsetY = -halfHeight- 0.04f;

        return Physics2D.Raycast(
                        new Vector2(transform.position.x, transform.position.y /*+ raycastOriginOffsetY*/),
                        direction,
                        groundCheckDistance,
                        environmentLayerMask
                );

	}

	public void Walk(float direction) {
	    	if(direction < 0){
	    		playerSprite.flipX = true;
	    	}else if(direction > 0){
	    		playerSprite.flipX = false;
	    	}

	        transform.Translate(new Vector2(direction * moveSpeed * Time.deltaTime, 0));
	}

	private class FlyingState : PlayerState {
		public static PlayerController _parent;

		public GameObject targetHold;
		private float horizontalInput = 0;
		private float airMultiplier = 1.5f;
		private float tongueMaxCastDistance = 20f;

		public FlyingState() {
			airMultiplier = 1.5f;
			targetHold = null;
		}

		public FlyingState(float newHorizontalInput){
			horizontalInput = newHorizontalInput;
			airMultiplier = 1.5f;
			targetHold = null;
		}

		public void setTargetHold(GameObject target) {
			if(target == null){
				_parent.marker.SetActive(false);
				return;
			}
			_parent.marker.SetActive(true);

			targetHold = target;
			_parent.marker.transform.position = target.transform.position + new Vector3(0, 1, 0);
		}

		public GameObject findClosestAnchor(){

			Vector3 playerPosition = _parent.transform.position;
			Vector2 hitScanDir = new Vector2(horizontalInput, 0);
			Collider2D[] hits = Physics2D.OverlapCircleAll(playerPosition, tongueMaxCastDistance, _parent.tongueLayerMask);
			float closestDistance = tongueMaxCastDistance + 1;
			GameObject closestObject = null;

			foreach(Collider2D hit in hits) {
				bool searchRight = horizontalInput > epsilon || horizontalInput < epsilon && _parent.playerRb.velocity.x > epsilon;
				bool isRight = hit.transform.position.x >= playerPosition.x;

				bool searchLeft = horizontalInput < epsilon || horizontalInput > epsilon && _parent.playerRb.velocity.x < epsilon;
				bool isLeft = hit.transform.position.x <= playerPosition.x;

				bool searchBoth = _parent.playerRb.velocity.x < epsilon;

				if((searchRight && isRight) || ( (searchLeft) && isLeft) || searchBoth) {


					float distance = Vector2.Distance(playerPosition, hit.transform.position);
					// only want to cast a ray if we know this is a good candidate
					// we cast this Ray to make sure nothing is blocking our tongue from attaching

					RaycastHit2D blockingCheck = Physics2D.Raycast(
						new Vector2(
							playerPosition.x + _parent.playerCollider.radius + 0.1f,
							playerPosition.y + _parent.playerCollider.radius + 0.1f
						),
						playerPosition - hit.transform.position,
						distance,
						1 << _parent.tongueLayerMask
					);

					if(distance < closestDistance  && !blockingCheck) {
						closestDistance = distance;
						closestObject = hit.gameObject;
					}
				}
			}

			return closestObject;
		}

		public void Update() {
			horizontalInput = Input.GetAxisRaw("Horizontal");

			GameObject newHold = findClosestAnchor();
			setTargetHold(newHold);

			if(_parent.walkEnabled && _parent.GroundCheck()){
				_parent.currentState = new WalkingState();
				Print.Log("STATE TRANSITION flying -> grounded");
				return;
			}
			// Hook Onto Target Hold
			if(Input.GetKeyDown(KeyCode.Space)){
				if(targetHold != null){
					Print.Log("STATE TRANSITION flying -> swinging");
					_parent.currentState = new SwingingState(targetHold);

					// Play hook audio
					AudioManager.instance.Play("hook");
				}
			}

			_parent.Walk(airMultiplier * horizontalInput);
		}

		public void FixedUpdate(){
			return;
		}
	}

	private class SwingingState : PlayerState {
		public static PlayerController _parent;

		private GameObject currentHold;


		private List<Vector2> tonguePositions = new List<Vector2>();
		private bool distanceSet;
		private Rigidbody2D tongueHingeAnchorRb;
		private SpriteRenderer tongueHingeAnchorSprite;

		private float horizontalInput;
		private float boostTimer;

		float minTongueLength = 3;
		float maxTongueLength = 20;

		public SwingingState(GameObject currentHold){
			this.currentHold = currentHold;

			_parent.transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
			tonguePositions.Add(currentHold.transform.position);
			_parent.tongueJoint.distance = Vector2.Distance(_parent.transform.position, currentHold.transform.position);
			_parent.tongueJoint.enabled = true;

			tongueHingeAnchorSprite = _parent.tongueHingeAnchor.GetComponent<SpriteRenderer>();

			tongueHingeAnchorRb = _parent.tongueHingeAnchor.GetComponent<Rigidbody2D>();
		}

		public void FixVelocity(){
			Vector2 oldVelocity = _parent.playerRb.velocity;

			Vector2 newDir;

			Vector2 playerToTongueJoint = new Vector2(
				_parent.transform.position.x - _parent.tongueJoint.transform.position.x,
				_parent.transform.position.y - _parent.tongueJoint.transform.position.y
			).normalized;

			Vector2 leftDirection = new Vector2(-playerToTongueJoint.x, playerToTongueJoint.y);
			Vector2 rightDirection = new Vector2(playerToTongueJoint.x, -playerToTongueJoint.y);

			if(Input.GetAxisRaw("Horizontal") < 0) {
				newDir = leftDirection;
			} else if(Input.GetAxisRaw("Horizontal") > 0) {
				newDir = rightDirection;
			} else {
				newDir = (_parent.playerRb.velocity.x < 0) ? leftDirection : rightDirection;
			}

			_parent.playerRb.velocity = newDir * _parent.playerRb.velocity.magnitude;
		}


		public void ChangeTongueLength(float direction){
			float rayCastLength = _parent.transform.localScale.y / 2 + 0.1f;

			RaycastHit2D hit = Physics2D.Raycast(
				_parent.playerRb.transform.position,
				new Vector2(0, direction),
				rayCastLength,
				_parent.environmentLayerMask
			);

			if(direction > 0 && _parent.tongueJoint.distance < maxTongueLength ||
				direction < 0 && _parent.tongueJoint.distance > minTongueLength) {
				_parent.tongueJoint.distance += _parent.tongueSpeed * direction;
			}
		}
		static int offset = 0; 
		public void UpdateTheFreakingTounge(){
			// Get Current Angle Around Circle
			// https://math.stackexchange.com/questions/830413/calculating-the-arc-length-of-a-circle-segment
			var radius = Vector2.Distance(tongueHingeAnchorRb.position,_parent.playerRb.position); 
			var distance = Vector2.Distance(tongueHingeAnchorRb.position + new Vector2(0, -radius), _parent.playerRb.position); 
			var theta = Mathf.Acos(1 - (distance * distance) / (2 * (radius * radius))) * Mathf.Rad2Deg;
			// Figure out what side we're on 
			
			if(_parent.playerRb.position.x < tongueHingeAnchorRb.position.x){
				theta = -theta; 
			}

			if(_parent.playerSprite.flipX){
				offset -= 20; 
			}else{
				offset += 20;
			}

			offset = Mathf.Clamp(offset, -30, 30);
			
			_parent.transform.rotation = Quaternion.Euler(Vector3.forward * (theta + offset));
			
		}

		public void Update() {
			// Update Player Movement
			UpdateTonguePositions();

			if(_parent.tongueJoint.distance < minTongueLength){
				ChangeTongueLength(Time.deltaTime);
			} else {
				ChangeTongueLength(-Input.GetAxis("Vertical") * Time.deltaTime);
			}

			if(Input.GetKeyUp(KeyCode.Space)){
				ResetTongue();
				Print.Log("STATE TRANSITION swinging -> flying");
				_parent.currentState = new FlyingState();
			}

			boostTimer += Time.deltaTime;
			UpdateTheFreakingTounge();

		}

        public void FixedUpdate()
        {
				Vector2 tongueHook = currentHold.transform.position;
				// 1 - Get a normalized direction vector from the player to the hook point
				var playerToHookDirection = (tongueHook - (Vector2)_parent.transform.position).normalized;

				// 2 - Inverse the direction to get a perpendicular direction
				Vector2 perpendicularDirection;
				horizontalInput = Input.GetAxisRaw("Horizontal");
				if (horizontalInput < 0)
				{
					perpendicularDirection = new Vector2(-playerToHookDirection.y, playerToHookDirection.x);
					var leftPerpPos = (Vector2)_parent.transform.position - perpendicularDirection * -2f;
					Debug.DrawLine(_parent.transform.position, leftPerpPos, Color.green, 0f);
					var force = perpendicularDirection * _parent.swingForce;
					_parent.playerRb.AddForce(force);
				}
				else if(horizontalInput > 0)
				{
					perpendicularDirection = new Vector2(playerToHookDirection.y, -playerToHookDirection.x);
					var rightPerpPos = (Vector2)_parent.transform.position + perpendicularDirection * 2f;
					Debug.DrawLine(_parent.transform.position, rightPerpPos, Color.green, 0f);
					var force = perpendicularDirection * _parent.swingForce;
					_parent.playerRb.AddForce(force);
				}
				UpdateTheFreakingTounge();

          }


        private void UpdateTonguePositions()
        {
            // 2
			var tongueRenderer = _parent.tongueRenderer;
            tongueRenderer.positionCount = tonguePositions.Count + 1;
            tongueHingeAnchorSprite.enabled = false;
            // 3
            for (var i = tongueRenderer.positionCount - 1; i >= 0; i--)
            {
                if (i != tongueRenderer.positionCount - 1) // if not the Last point of line renderer
                {
                    tongueRenderer.SetPosition(i, tonguePositions[i]);

                    // 4
                    if (i == tonguePositions.Count - 1 || tonguePositions.Count == 1)
                    {
                        var tonguePosition = tonguePositions[tonguePositions.Count - 1];
                        if (tonguePositions.Count == 1)
                        {
                            tongueHingeAnchorRb.transform.position = tonguePosition;
                            if (!distanceSet)
                            {
                                _parent.tongueJoint.distance = Vector2.Distance(_parent.transform.position, tonguePosition);
                                distanceSet = true;
                            }
                        }
                        else
                        {
                            tongueHingeAnchorRb.transform.position = tonguePosition;
                            if (!distanceSet)
                            {
                                _parent.tongueJoint.distance = Vector2.Distance(_parent.transform.position, tonguePosition);
                                distanceSet = true;
                            }
                        }
                    }
                    // 5
                    else if (i - 1 == tonguePositions.IndexOf(tonguePositions.Last()))
                    {
                        var tonguePosition = tonguePositions.Last();
                        tongueHingeAnchorRb.transform.position = tonguePosition;
                        if (!distanceSet)
                        {
                            _parent.tongueJoint.distance = Vector2.Distance(_parent.transform.position, tonguePosition);
                            distanceSet = true;
                        }
                    }
                }
                else
                {
                    // 6
                    tongueRenderer.SetPosition(i, _parent.transform.position);
                }
            }
            //tongueHingeAnchorSprite.enabled = true;
        }

        public void ResetTongue()
        {
            _parent.tongueJoint.enabled = false;
            _parent.tongueRenderer.positionCount = 2;
            _parent.tongueRenderer.SetPosition(0, _parent.transform.position);
            _parent.tongueRenderer.SetPosition(1, _parent.transform.position);
            tonguePositions.Clear();
            tongueHingeAnchorSprite.enabled = false;

			if(boostTimer > 0.25f){
				_parent.playerRb.velocity *= _parent.boostSpeed;
			}
        }
	}

	private class WalkingState : PlayerState {
		public static PlayerController _parent;
		private float horizontalInput;

		public WalkingState(){
			//empty constructor
		}

		public void Update() {

			horizontalInput = Input.GetAxisRaw("Horizontal");

	        FlyingState fs = new FlyingState(horizontalInput);
	        fs.setTargetHold(fs.findClosestAnchor());
	        SwingingState ss;

	        checkValidity();

	        if(!_parent.GroundCheck()){
	        	Print.Log("STATE TRANSITION grounded -> flying");
	        	_parent.currentState = fs;
	        	return;
	        }

	        if(Input.GetKeyDown(KeyCode.Space)){
	        	if(fs.targetHold != null ){
	        		Print.Log("STATE TRANSITION grounded -> swinging");
	        		ss = new SwingingState(fs.targetHold);
		        	_parent.currentState = ss;

					// Play hook audio
					AudioManager.instance.Play("hook");
	        	}
	        }

	        _parent.Walk(horizontalInput);
    	}

	    public void FixedUpdate(){
	    	return;
	    }

		private void checkValidity(){
			if(!_parent.walkEnabled){
				Print.Log("ERROR: walk disabled but in walking state");
			}
		}
	}
	// STATE WIRING
	// ------------------------------------------------
	// Use this for initialization
	float horizontalInput = 0;
	void Start () {
		// Initialize Static References
		FlyingState._parent = this;
		SwingingState._parent = this;
		WalkingState._parent = this;

		walkEnabled = true;
		currentState = new FlyingState();
		playerRb = GetComponent<Rigidbody2D>();
		playerSprite = GetComponent<SpriteRenderer>();
		playerCollider = GetComponent<CircleCollider2D>();

		defaultColor = playerSprite.color;
		defaultGravityScale = playerRb.gravityScale;
	}

	// Update is called once per frame
	void Update () {
		currentState.Update();

		horizontalInput = Input.GetAxis("Horizontal");

		if( horizontalInput != 0 ){
			GetComponent<SpriteRenderer>().flipX = horizontalInput < 0;
		}

		var amt = 4;
		if(!(currentState is SwingingState)){
			// Return to normal rotation
			// var curAngle = this.transform.rotation.eulerAngles.z;
			// if(curAngle != 0){
			// 	var newAngle = curAngle + ((curAngle > 0) ?  -amt : amt);
			// 	newAngle = (curAngle > 0) ? Mathf.Floor(newAngle) : Mathf.Ceil(newAngle); 
			// 	if(curAngle > 0 && newAngle < 0 || curAngle < 0 && newAngle > 0){
			// 		newAngle = 0;
			// 	}

				this.transform.rotation = Quaternion.Euler(0, 0, 0);
			// }
		}
	}

	void LateUpdate() {
		if(animator != null && animator.enabled){
			setAnimatorState(horizontalInput);	
		}
	}

	void FixedUpdate() {
		currentState.FixedUpdate();
	}

	public IEnumerator Respawn() {
		float deathWaitTime = 1f;
		int deathSpins = 2;

		// change things
		this.playerRb.gravityScale = 0;
		this.playerSprite.color = Color.black;
		this.playerRb.velocity = Vector2.zero;
		this.playerCollider.enabled = false;

		// logic
		
		var checkpoint = GameManager.instance.CurrentCheckpoint;
		if(checkpoint == null){
			checkpoint = GameManager.instance.levelStart;
		}

		// spin setup
		float spinTime = deathWaitTime / (float)deathSpins;
		// single spin, fade while we spin
		Quaternion startRotation = transform.rotation;
		
		for(float t = 0; t < deathWaitTime; t+=Time.deltaTime) {
			transform.rotation = startRotation * Quaternion.AngleAxis(t / spinTime * 360f * deathSpins, Vector2.up);
			this.playerRb.velocity = Vector2.zero;
			
			ResetTongueIfSwinging();

			yield return null;
		}

		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.rotation = startRotation;

		// teleport
		transform.position = checkpoint.respawnPoint.transform.position;

		// change back what we had before
		this.playerRb.gravityScale = defaultGravityScale;
		this.playerSprite.color = defaultColor;
		this.playerCollider.enabled = true;
		
		currentState = new FlyingState();
	}

	public void setABValue(){
		abValue = GameManager.instance.abValue;
		Print.Log("[PC] " + abValue);
		swingForce = abValue == 1 ? 8 : 12; 
	}

	public void setAnimatorState(float horizontalInput){
		int stateNumber = 0;
		if(currentState is WalkingState){
			stateNumber = Mathf.Abs(horizontalInput) > epsilon ? 1 : 0;
		}
		else if(currentState is FlyingState){
			//stateNumber = 2;
		}else if(currentState is SwingingState){
			stateNumber = 2;
		}

		animator.SetInteger("State", stateNumber);
	}

	public void ResetTongueIfSwinging(){
		if(currentState is SwingingState){
			SwingingState swingingState = (SwingingState)currentState;
			swingingState.ResetTongue();
		}
	}
}
