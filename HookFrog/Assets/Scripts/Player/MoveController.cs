using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class MoveController : MonoBehaviour {
    public float swingForce = 4f;
    public float moveSpeed = 1f;
    public float jumpSpeed = 3f;

    public Vector2 tongueHook;
    public bool isSwinging;
    public bool groundCheck;
    private SpriteRenderer playerSprite;
    private Rigidbody2D rb;
    private TongueController tongueController;
    private CircleCollider2D collider;

    void Awake() {
        playerSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        tongueController = GetComponent<TongueController>();
        collider = GetComponent<CircleCollider2D>();
    }

    void Update() {
        float angle = Mathf.Sqrt(2) / 2;

        GroundCheck(Vector2.down);
        GroundCheck(new Vector2(angle, -angle));
        GroundCheck(new Vector2(-angle, -angle));
    }

    void GroundCheck(Vector2 direction){
        float groundCheckDistance = 0.025f;

        groundCheck = Physics2D.OverlapCircle(
            transform.position, 
            collider.radius + groundCheckDistance, 
            tongueController.stickLayer | tongueController.environmentLayer
        );
    }

    public void Move(float direction) {
        playerSprite.flipX = direction < 0f;

        if (isSwinging && direction != 0) {
            Swing(direction);
        }
        else {
            Walk(direction);
        } 
    }

    private void Walk(float direction) {
        if(groundCheck && direction == 0) {
            rb.velocity = new Vector2(0, rb.velocity.y); 
        } else {
            transform.Translate(new Vector2(direction * moveSpeed * Time.deltaTime, 0));
        }
    }

    private void Swing(float direction) {

        // Get normalized direction vector from player to the hook point
        Vector2 playerToHookDirection = (tongueHook - (Vector2)transform.position).normalized;

        // Inverse the direction to get a perpendicular direction
        Vector2 perpendicularDirection;
        float directionFactor = 2f;
        if (direction < 0) {
            perpendicularDirection = new Vector2(-playerToHookDirection.y, playerToHookDirection.x);
            Vector2 leftPerpPos = (Vector2)transform.position - perpendicularDirection * -directionFactor;
            Debug.DrawLine(transform.position, leftPerpPos, Color.green, 0f);
        }
        else {
            perpendicularDirection = new Vector2(playerToHookDirection.y, -playerToHookDirection.x);
            Vector2 rightPerpPos = (Vector2)transform.position + perpendicularDirection * directionFactor;
            Debug.DrawLine(transform.position, rightPerpPos, Color.green, 0f);
        }

        // the direction of our velocity is the perpendicular direction, 
        // the magnitude is our swingForce
        Vector2 force = perpendicularDirection * swingForce;
       
        rb.AddForce(force, ForceMode2D.Force);
    }
    
    public void Jump() {
        Debug.Log("Jump1");
        if (!groundCheck) return;
        Debug.Log("Jump2");
        
        rb.AddForce(new Vector2(0, jumpSpeed));
    }
}